namespace DbMercado.Application.Produto.Interfaces;

/// <summary>
/// Persistência física de arquivos de mídia (wwwroot/uploads ou storage externo).
/// </summary>
public interface IMidiaArquivoStorage
{
    /// <summary>Retorna URL relativa (ex.: /uploads/produtos/2026/04/arquivo.jpg) e caminho físico.</summary>
    Task<(string RelativeUrl, string PhysicalPath)> SalvarUploadAsync(
        Stream conteudo,
        string extensaoNormalizada,
        CancellationToken cancellationToken = default);

    void ExcluirPorUrlRelativa(string urlRelativa);
}
