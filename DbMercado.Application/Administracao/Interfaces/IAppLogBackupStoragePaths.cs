namespace DbMercado.Application.Administracao.Interfaces;

/// <summary>
/// Resolve o diretório físico para gravação e leitura de backups de log.
/// </summary>
public interface IAppLogBackupStoragePaths
{
    /// <summary>Diretório absoluto; a implementação deve garantir que exista quando necessário.</summary>
    string ObterDiretorioBackupAbsoluto();
}
