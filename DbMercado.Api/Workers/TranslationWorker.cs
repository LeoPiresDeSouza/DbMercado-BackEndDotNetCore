using System.Diagnostics;
using System.Threading.Channels;
using DbMercado.Application.Chat;
using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.HubClients;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.Api.Hubs;
using DbMercado.Domain.Chat;
using Microsoft.AspNetCore.SignalR;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;

namespace DbMercado.Api.Workers;

/// <summary>Consome <see cref="TranslationJob"/> (Etapa 4) e reprocessa mensagens <c>Pending</c> no startup (mesma etapa).</summary>
public sealed class TranslationWorker : BackgroundService
{
    public const string UsuarioAuditoriaSistema = "sistema";

    private readonly ChannelReader<TranslationJob> _reader;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;
    private readonly IOpenRouterTranslationService _openRouter;
    private readonly ITranslationCache _translationCache;
    private readonly ILogger<TranslationWorker> _logger;

    public TranslationWorker(
        ChannelReader<TranslationJob> reader,
        IServiceScopeFactory scopeFactory,
        IHubContext<ChatHub, IChatHubClient> hubContext,
        IOpenRouterTranslationService openRouter,
        ITranslationCache translationCache,
        ILogger<TranslationWorker> logger)
    {
        _reader = reader;
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _openRouter = openRouter;
        _translationCache = translationCache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ReprocessarPendentesNoStartupAsync(stoppingToken);

        try
        {
            await foreach (var job in _reader.ReadAllAsync(stoppingToken))
                await ProcessarJobAsync(job, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Encerramento da aplicação.
        }
    }

    private async Task ReprocessarPendentesNoStartupAsync(CancellationToken cancellationToken)
    {
        List<MessageEntity> snapshot;
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var uw = scope.ServiceProvider.GetRequiredService<IUwChat>();
            snapshot = (await uw.Mensagens.ListarMensagensTraducaoPendenteAsync(cancellationToken)).ToList();
        }

        if (snapshot.Count == 0)
            return;

        _logger.LogInformation(
            "Chat tradução: reprocessando {Count} mensagem(ns) com status pendente.",
            snapshot.Count);

        foreach (var msg in snapshot)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var uw = scope.ServiceProvider.GetRequiredService<IUwChat>();
            var targets = await ChatTranslationTargets.ResolverParaSalaAsync(
                uw,
                msg.RoomId,
                msg.SourceLang,
                cancellationToken);

            var job = new TranslationJob(
                msg.Id,
                msg.ContentOriginal,
                msg.SourceLang,
                targets,
                msg.RoomId);

            await ProcessarJobComUwAsync(uw, job, cancellationToken);
        }
    }

    private async Task ProcessarJobAsync(TranslationJob job, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uw = scope.ServiceProvider.GetRequiredService<IUwChat>();
        await ProcessarJobComUwAsync(uw, job, cancellationToken);
    }

    private async Task ProcessarJobComUwAsync(
        IUwChat uw,
        TranslationJob job,
        CancellationToken cancellationToken)
    {
        var message = await uw.Mensagens.ObterPorIdComTrackingAsync(job.MessageId, cancellationToken);
        if (message is null || message.DeletedAt is not null)
            return;

        if (message.TranslationStatus != MessageTranslationStatus.Pending)
            return;

        if (!LanguageCode.TryNormalize(message.SourceLang, out var sourceCanon))
            sourceCanon = message.SourceLang.Trim();

        var targets = job.TargetLangs
            .Select(l => LanguageCode.TryNormalize(l, out var c) ? c : null)
            .Where(c => c is not null && !string.Equals(c, sourceCanon, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Cast<string>()
            .ToList();

        var membros = await uw.Membros.ListarUsuarioEIdiomaPorSalaAsync(job.RoomId, cancellationToken);
        var membrosCanon = membros
            .Select(m => (m.UserId, Lang: LanguageCode.TryNormalize(m.LanguagePref, out var c) ? c : (string?)null))
            .Where(x => x.Lang is not null)
            .Select(x => (x.UserId, Lang: x.Lang!))
            .ToList();

        if (targets.Count == 0)
        {
            _logger.LogInformation(
                "Chat tradução: job MessageId={MessageId} sem idiomas alvo após normalização; marcando como concluída.",
                job.MessageId);
            message.TranslationStatus = MessageTranslationStatus.Done;
            await uw.Mensagens.AtualizarAsync(UsuarioAuditoriaSistema, message, cancellationToken);
            await uw.SaveChangesAsync(cancellationToken);
            return;
        }

        _logger.LogInformation(
            "Chat tradução: job recebido MessageId={MessageId} TargetLangs=[{TargetLangs}]",
            job.MessageId,
            string.Join(", ", targets));

        foreach (var targetLang in targets)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (await uw.Mensagens.TraducaoExisteAsync(job.MessageId, targetLang, cancellationToken))
                continue;

            string translated;
            var fromCache = false;
            var latenciaMsOpenRouter = 0L;

            try
            {
                var textHash = TranslationTextHash.Compute(message.ContentOriginal, targetLang);
                var cached = await _translationCache.GetAsync(textHash, targetLang, cancellationToken);
                if (cached is not null)
                {
                    translated = cached;
                    fromCache = true;
                    _logger.LogInformation(
                        "Chat tradução: cache hit MessageId={MessageId} TargetLang={TargetLang}",
                        job.MessageId,
                        targetLang);
                }
                else
                {
                    _logger.LogInformation(
                        "Chat tradução: cache miss MessageId={MessageId} TargetLang={TargetLang}",
                        job.MessageId,
                        targetLang);

                    var swAi = Stopwatch.StartNew();
                    try
                    {
                        translated = await _openRouter.TranslateAsync(
                            message.ContentOriginal,
                            sourceCanon,
                            targetLang,
                            cancellationToken);
                        latenciaMsOpenRouter = swAi.ElapsedMilliseconds;
                        _logger.LogInformation(
                            "Chat tradução: OpenRouter MessageId={MessageId} TargetLang={TargetLang} LatenciaMs={LatenciaMs}",
                            job.MessageId,
                            targetLang,
                            latenciaMsOpenRouter);
                    }
                    catch (Exception)
                    {
                        latenciaMsOpenRouter = swAi.ElapsedMilliseconds;
                        throw;
                    }

                    await _translationCache.SetAsync(
                        textHash,
                        targetLang,
                        translated,
                        TranslationCacheDefaults.EntryTtl,
                        cancellationToken);
                }

                var entity = new MessageTranslationEntity
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    TargetLang = targetLang,
                    TranslatedText = translated,
                    FromCache = fromCache,
                    TranslatedAt = DateTime.UtcNow
                };

                await uw.Mensagens.AdicionarTraducaoAsync(UsuarioAuditoriaSistema, entity, cancellationToken);
                await uw.SaveChangesAsync(cancellationToken);

                await NotificarTraducaoAsync(
                    message.Id,
                    targetLang,
                    translated,
                    membrosCanon,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Chat tradução: falha definitiva (retentativas HTTP Polly esgotadas ou erro de negócio) MessageId={MessageId} TargetLang={TargetLang} LatenciaMs={LatenciaMs}",
                    job.MessageId,
                    targetLang,
                    latenciaMsOpenRouter);

                message.TranslationStatus = MessageTranslationStatus.Failed;
                await uw.Mensagens.AtualizarAsync(UsuarioAuditoriaSistema, message, cancellationToken);
                await uw.SaveChangesAsync(cancellationToken);

                await NotificarFalhaTraducaoParaAlvosSemTraducaoAsync(
                    uw,
                    message.Id,
                    targets,
                    membrosCanon,
                    cancellationToken);
                return;
            }
        }

        message.TranslationStatus = MessageTranslationStatus.Done;
        await uw.Mensagens.AtualizarAsync(UsuarioAuditoriaSistema, message, cancellationToken);
        await uw.SaveChangesAsync(cancellationToken);
    }

    private async Task NotificarTraducaoAsync(
        Guid messageId,
        string targetLang,
        string translatedText,
        IReadOnlyList<(string UserId, string Lang)> membrosCanon,
        CancellationToken cancellationToken)
    {
        var userIds = membrosCanon
            .Where(m => string.Equals(m.Lang, targetLang, StringComparison.Ordinal))
            .Select(m => m.UserId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        foreach (var userId in userIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _hubContext.Clients.User(userId).ReceiveTranslation(
                messageId,
                translatedText,
                targetLang);
        }
    }

    /// <summary>
    /// Para cada idioma-alvo do job sem linha persistida em <c>MessageTranslations</c>, notifica
    /// <see cref="IChatHubClient.TranslationFailed"/> aos membros daquela preferência — cobre o idioma que falhou
    /// e os que ainda não seriam processados após o <c>return</c> do <c>catch</c>.
    /// </summary>
    private async Task NotificarFalhaTraducaoParaAlvosSemTraducaoAsync(
        IUwChat uw,
        Guid messageId,
        IReadOnlyList<string> targetLangs,
        IReadOnlyList<(string UserId, string Lang)> membrosCanon,
        CancellationToken cancellationToken)
    {
        foreach (var lang in targetLangs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await uw.Mensagens.TraducaoExisteAsync(messageId, lang, cancellationToken))
                continue;

            await NotificarFalhaTraducaoAsync(messageId, lang, membrosCanon, cancellationToken);
        }
    }

    /// <summary>
    /// Notifica membros cuja preferência na sala coincide com <paramref name="failedTargetLang"/>.
    /// </summary>
    private async Task NotificarFalhaTraducaoAsync(
        Guid messageId,
        string failedTargetLang,
        IReadOnlyList<(string UserId, string Lang)> membrosCanon,
        CancellationToken cancellationToken)
    {
        var userIds = membrosCanon
            .Where(m => string.Equals(m.Lang, failedTargetLang, StringComparison.OrdinalIgnoreCase))
            .Select(m => m.UserId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        foreach (var userId in userIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _hubContext.Clients.User(userId).TranslationFailed(messageId);
        }
    }
}
