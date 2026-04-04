using DbMercado.Application.Produto.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace DbMercado.Infrastructure.Produto.Services;

public sealed class MidiaArquivoStorage : IMidiaArquivoStorage
{
    private readonly IWebHostEnvironment _env;

    public MidiaArquivoStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <inheritdoc />
    public async Task<(string RelativeUrl, string PhysicalPath)> SalvarUploadAsync(
        Stream conteudo,
        string extensaoNormalizada,
        CancellationToken cancellationToken = default)
    {
        var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        var agora = DateTime.UtcNow;
        var ano = agora.ToString("yyyy");
        var mes = agora.ToString("MM");
        var guid = Guid.NewGuid().ToString("N");
        var ext = extensaoNormalizada.StartsWith('.') ? extensaoNormalizada : "." + extensaoNormalizada;
        var nomeArquivo = $"{guid}{ext}";
        var fisDir = Path.Combine(webRoot, "uploads", "produtos", ano, mes);
        Directory.CreateDirectory(fisDir);
        var physicalPath = Path.Combine(fisDir, nomeArquivo);
        await using (var fs = File.Create(physicalPath))
        {
            await conteudo.CopyToAsync(fs, cancellationToken);
        }

        var relativeUrl = $"/uploads/produtos/{ano}/{mes}/{nomeArquivo}";
        return (relativeUrl, physicalPath);
    }

    /// <inheritdoc />
    public void ExcluirPorUrlRelativa(string urlRelativa)
    {
        if (string.IsNullOrWhiteSpace(urlRelativa) || !urlRelativa.StartsWith("/uploads/", StringComparison.Ordinal))
            return;

        var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;
        var trimmed = urlRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var full = Path.GetFullPath(Path.Combine(webRoot, trimmed));
        var rootFull = Path.GetFullPath(webRoot);
        if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            return;

        if (File.Exists(full))
            File.Delete(full);
    }
}
