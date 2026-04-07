using System.Threading.Channels;
using DbMercado.Api.Authorization;
using DbMercado.Api.Extensions;
using DbMercado.Application.Chat;
using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.HubClients;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DbMercado.Api.Hubs;

[Authorize(Policy = ChatMultilingueAcessarRequirement.PolicyName)]
public sealed class ChatHub : Hub<IChatHubClient>
{
    private readonly IChatSalaService _chatSalaService;
    private readonly IChatMensagemService _chatMensagemService;
    private readonly IChatMembroService _chatMembroService;
    private readonly ChannelWriter<TranslationJob> _translationJobsWriter;
    private readonly IChatMensagemEnvioRateLimiter _envioRateLimiter;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IChatSalaService chatSalaService,
        IChatMensagemService chatMensagemService,
        IChatMembroService chatMembroService,
        ChannelWriter<TranslationJob> translationJobsWriter,
        IChatMensagemEnvioRateLimiter envioRateLimiter,
        ILogger<ChatHub> logger)
    {
        _chatSalaService = chatSalaService;
        _chatMensagemService = chatMensagemService;
        _chatMembroService = chatMembroService;
        _translationJobsWriter = translationJobsWriter;
        _envioRateLimiter = envioRateLimiter;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "Conexão SignalR chat recusada: usuário autenticado sem identificador no token (ConnectionId={ConnectionId}).",
                Context.ConnectionId);
            Context.Abort();
            return;
        }

        var salas = await _chatSalaService.ListarMinhasSalasAsync(userId, Context.ConnectionAborted);

        foreach (var sala in salas)
            await Groups.AddToGroupAsync(Context.ConnectionId, sala.Id.ToString());

        await base.OnConnectedAsync();
    }

    /// <summary>Envia mensagem de texto à sala; todos os membros no grupo SignalR recebem <see cref="IChatHubClient.ReceiveMessage"/>.</summary>
    public async Task SendMessage(SendMessageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = Context.User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            throw new HubException("Usuário não identificado.");

        if (!_envioRateLimiter.TryAcquire(userId, out var retrySeg))
        {
            throw new HubException(
                $"Limite de envio: no máximo {ChatMensagemEnvioRateLimitPolicy.MaxPorJanela} mensagens por minuto. Tente novamente em {retrySeg} segundo(s).");
        }

        try
        {
            var dto = await _chatMensagemService.EnviarMensagemAsync(
                userId,
                Context.User.ResolveUsuarioAuditoria(),
                request,
                Context.ConnectionAborted);

            await Clients.Group(request.RoomId.ToString()).ReceiveMessage(dto);

            var idiomasAlvo = await _chatMembroService.ObterIdiomasAlvoTraducaoAsync(
                request.RoomId,
                dto.SourceLang,
                Context.ConnectionAborted);

            if (idiomasAlvo.Count > 0)
            {
                var job = new TranslationJob(
                    dto.MessageId,
                    dto.Content,
                    dto.SourceLang,
                    idiomasAlvo,
                    request.RoomId);

                await _translationJobsWriter.WriteAsync(job, Context.ConnectionAborted);
            }
        }
        catch (BusinessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    /// <summary>Registra recibo de leitura e notifica o grupo da sala via <see cref="IChatHubClient.MessageRead"/>.</summary>
    public async Task MarkAsRead(Guid messageId)
    {
        var userId = Context.User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            throw new HubException("Usuário não identificado.");

        try
        {
            var roomId = await _chatMensagemService.MarcarComoLidaAsync(
                userId,
                Context.User.ResolveUsuarioAuditoria(),
                messageId,
                Context.ConnectionAborted);

            await Clients.Group(roomId.ToString()).MessageRead(messageId, userId);
        }
        catch (BusinessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    /// <summary>Indica digitação na sala; broadcast ao grupo sem persistência, após validar membro e sala ativa.</summary>
    public async Task UserTyping(Guid roomId, bool isTyping)
    {
        var userId = Context.User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            throw new HubException("Usuário não identificado.");

        try
        {
            await _chatMembroService.ValidarMembroSalaAtivaAsync(
                userId,
                roomId,
                Context.ConnectionAborted);

            await Clients.Group(roomId.ToString()).UserTyping(roomId, userId, isTyping);
        }
        catch (BusinessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            throw new HubException(ex.Message);
        }
    }
}
