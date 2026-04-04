namespace DbMercado.CrossCutting.Settings;

/// <summary>
/// Pasta relativa ao <c>wwwroot</c> onde arquivos de backup de log são gravados (<c>appsettings:Storage:LogBackup</c>).
/// </summary>
public sealed class LogBackupStorageOptions
{
    public const string SectionName = "Storage:LogBackup";

    /// <summary>Caminho relativo ao web root (ex.: <c>logs/backup</c>).</summary>
    public string Pasta { get; set; } = "logs/backup";
}
