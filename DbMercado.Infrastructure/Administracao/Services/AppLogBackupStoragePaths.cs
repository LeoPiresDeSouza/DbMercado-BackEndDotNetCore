using DbMercado.Application.Administracao.Interfaces;
using DbMercado.CrossCutting.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace DbMercado.Infrastructure.Administracao.Services;

public sealed class AppLogBackupStoragePaths : IAppLogBackupStoragePaths
{
    private readonly IWebHostEnvironment _env;
    private readonly LogBackupStorageOptions _options;

    public AppLogBackupStoragePaths(IWebHostEnvironment env, IOptions<LogBackupStorageOptions> options)
    {
        _env = env;
        _options = options.Value;
    }

    public string ObterDiretorioBackupAbsoluto()
    {
        var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        var relative = _options.Pasta.Trim().TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var full = Path.GetFullPath(Path.Combine(webRoot, relative));
        var rootFull = Path.GetFullPath(webRoot);
        if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage:LogBackup:Pasta inválida (fora do web root).");

        Directory.CreateDirectory(full);
        return full;
    }
}
