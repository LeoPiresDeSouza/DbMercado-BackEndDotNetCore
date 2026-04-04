using DbMercado.Application.Administracao.Dtos.AppLog;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Domain.Shared.Exceptions;
using DbMercado.Domain.Shared.Interfaces.Repositories;

namespace DbMercado.Application.Administracao.Services;

public sealed class AppLogService : IAppLogService
{
    private const string ParamCategoria = "Log";
    private const string ParamAtributo = "Limpeza";

    private readonly IAppLogRepository _appLog;
    private readonly IParametroChaveConsultaRepository _parametros;
    private readonly IAppLogBackupStoragePaths _backupPaths;

    public AppLogService(
        IAppLogRepository appLog,
        IParametroChaveConsultaRepository parametros,
        IAppLogBackupStoragePaths backupPaths)
    {
        _appLog = appLog;
        _parametros = parametros;
        _backupPaths = backupPaths;
    }

    public async Task<AppLogGridResultDto> ConsultarGridAsync(AppLogGridQueryDto query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var take = Math.Clamp(query.EndRow - query.StartRow, 0, 500);
        var skip = Math.Max(0, query.StartRow);
        var (col, desc) = ResolverOrdenacao(query.SortModel);

        var (rows, total) = await _appLog.ConsultarGridAsync(
            query.DataInicio,
            query.DataFim,
            query.SomenteComExcecao,
            query.Levels,
            skip,
            take,
            col,
            desc,
            cancellationToken);

        return new AppLogGridResultDto
        {
            RowCount = total,
            Rows = rows.Select(e => new AppLogGridRowDto
            {
                Id = e.Id,
                CreatedAt = e.CreatedAt,
                Level = e.Level,
                Category = e.Category,
                Message = e.Message,
                HasException = e.HasException,
                UserName = e.UserName,
                Path = e.Path,
                Method = e.Method
            }).ToList()
        };
    }

    public async Task<AppLogDetalheDto?> ObterDetalheAsync(long id, CancellationToken cancellationToken = default)
    {
        var e = await _appLog.ObterPorIdAsync(id, cancellationToken);
        if (e is null)
            return null;

        return new AppLogDetalheDto
        {
            Id = e.Id,
            Category = e.Category,
            Level = e.Level,
            Message = e.Message,
            Exception = e.Exception,
            ErrorCode = e.ErrorCode,
            TraceId = e.TraceId,
            UserId = e.UserId,
            UserName = e.UserName,
            Path = e.Path,
            Method = e.Method,
            Ip = e.Ip,
            UserAgent = e.UserAgent,
            CreatedAt = e.CreatedAt
        };
    }

    public Task<bool> ExcluirEntradaAsync(long id, CancellationToken cancellationToken = default) =>
        _appLog.ExcluirPorIdAsync(id, cancellationToken);

    public async Task<LogLimpezaResultDto> ExecutarLimpezaAsync(CancellationToken cancellationToken = default)
    {
        var lista = await _parametros.ListarPorCategoriaEAtributoAsync(ParamCategoria, ParamAtributo, cancellationToken);
        var minimo = LerIntParametro(lista, "MinimoRegistros", 50);
        var maximo = LerIntParametro(lista, "MaximoRegistros", 100);
        var diasLimpeza = Math.Clamp(LerIntParametro(lista, "DiasLimpeza", 1), 1, 3650);

        minimo = Math.Max(0, minimo);
        maximo = Math.Max(0, maximo);
        if (maximo < minimo)
            (minimo, maximo) = (maximo, minimo);

        var total = await _appLog.ContarTotalAsync(cancellationToken);
        if (total <= maximo)
        {
            await LimparArquivosBackupAntigosAsync(diasLimpeza, cancellationToken);
            return new LogLimpezaResultDto { RegistrosExcluidos = 0, ArquivoBackup = null };
        }

        var qtdExcluir = total - minimo;
        if (qtdExcluir <= 0)
        {
            await LimparArquivosBackupAntigosAsync(diasLimpeza, cancellationToken);
            return new LogLimpezaResultDto { RegistrosExcluidos = 0, ArquivoBackup = null };
        }

        var registros = await _appLog.ObterMaisAntigosAsync(qtdExcluir, cancellationToken);
        if (registros.Count == 0)
        {
            await LimparArquivosBackupAntigosAsync(diasLimpeza, cancellationToken);
            return new LogLimpezaResultDto { RegistrosExcluidos = 0, ArquivoBackup = null };
        }

        var dir = _backupPaths.ObterDiretorioBackupAbsoluto();
        var nomeArquivo = $"logBkp_{DateTime.UtcNow:ddMMyyyy-HHmmss}.txt";
        var caminho = Path.Combine(dir, nomeArquivo);

        await using (var writer = new StreamWriter(new FileStream(caminho, FileMode.CreateNew, FileAccess.Write, FileShare.None), System.Text.Encoding.UTF8))
        {
            foreach (var e in registros)
            {
                var ex = e.Exception?.Replace('\r', ' ').Replace('\n', ' ') ?? string.Empty;
                await writer.WriteLineAsync(
                    $"[{e.CreatedAt:O}]  {e.Level}  {e.Category}  {e.Message}  {ex}");
            }
        }

        var ids = registros.Select(r => r.Id).ToList();
        var deleted = await _appLog.ExcluirPorIdsAsync(ids, cancellationToken);

        await LimparArquivosBackupAntigosAsync(diasLimpeza, cancellationToken);

        return new LogLimpezaResultDto
        {
            RegistrosExcluidos = deleted,
            ArquivoBackup = nomeArquivo
        };
    }

    public Task<IReadOnlyList<LogBackupArquivoDto>> ListarBackupsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dir = _backupPaths.ObterDiretorioBackupAbsoluto();
        if (!Directory.Exists(dir))
            return Task.FromResult<IReadOnlyList<LogBackupArquivoDto>>([]);

        var infos = new DirectoryInfo(dir).GetFiles("*.txt")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new LogBackupArquivoDto
            {
                Nome = f.Name,
                TamanhoBytes = f.Length,
                DataCriacao = new DateTimeOffset(f.CreationTimeUtc, TimeSpan.Zero)
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<LogBackupArquivoDto>>(infos);
    }

    public async Task<(string Conteudo, string NomeArquivo)?> ObterConteudoBackupAsync(
        string nomeArquivo,
        CancellationToken cancellationToken = default)
    {
        if (!NomeArquivoBackupEhSeguro(nomeArquivo))
            throw new BusinessException("APP_LOG_BACKUP_NOME_INVALIDO", "Nome de arquivo de backup inválido.");

        var dir = _backupPaths.ObterDiretorioBackupAbsoluto();
        var path = Path.Combine(dir, nomeArquivo);
        var full = Path.GetFullPath(path);
        if (!full.StartsWith(Path.GetFullPath(dir), StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            return null;

        var conteudo = await File.ReadAllTextAsync(full, cancellationToken);
        return (conteudo, nomeArquivo);
    }

    private Task LimparArquivosBackupAntigosAsync(int diasRetencao, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dir = _backupPaths.ObterDiretorioBackupAbsoluto();
        if (!Directory.Exists(dir))
            return Task.CompletedTask;

        var cutoff = DateTime.UtcNow.AddDays(-diasRetencao);
        foreach (var file in new DirectoryInfo(dir).EnumerateFiles("*.txt"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.CreationTimeUtc < cutoff)
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // Não falha a limpeza principal por arquivo travado
                }
            }
        }

        return Task.CompletedTask;
    }

    private static bool NomeArquivoBackupEhSeguro(string nomeArquivo)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            return false;
        if (nomeArquivo.Contains('\\', StringComparison.Ordinal) || nomeArquivo.Contains('/', StringComparison.Ordinal))
            return false;
        if (nomeArquivo.Contains("..", StringComparison.Ordinal))
            return false;
        return string.Equals(Path.GetFileName(nomeArquivo), nomeArquivo, StringComparison.Ordinal);
    }

    private static int LerIntParametro(IReadOnlyList<(string Chave, string Valor)> lista, string chave, int padrao)
    {
        foreach (var (k, v) in lista)
        {
            if (k == chave && int.TryParse(v, out var n))
                return n;
        }

        return padrao;
    }

    private static (string Column, bool Descending) ResolverOrdenacao(List<AppLogGridSortItemDto>? sortModel)
    {
        if (sortModel is not { Count: > 0 })
            return ("createdAt", true);

        var first = sortModel[0];
        var col = (first.ColId ?? string.Empty).Trim().ToLowerInvariant();
        var desc = string.Equals(first.Sort, "desc", StringComparison.OrdinalIgnoreCase);

        return col switch
        {
            "createdat" => ("createdAt", desc),
            "level" => ("level", desc),
            "category" => ("category", desc),
            "message" => ("message", desc),
            "username" => ("userName", desc),
            "path" => ("path", desc),
            "method" => ("method", desc),
            _ => ("createdAt", desc)
        };
    }
}
