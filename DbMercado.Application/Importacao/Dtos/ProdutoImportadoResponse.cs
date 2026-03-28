namespace DbMercado.Application.Importacao.Dtos;

public class ProdutoImportadoResponse
{
    public long Id { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Ncm { get; set; }
    public string? UnidadeMedida { get; set; }
    public long? ItemNotaFiscalOrigemId { get; set; }
    public long? NotaFiscalOrigemId { get; set; }
    public string? NotaFiscalChaveAcesso { get; set; }
}

public class ProdutoImportadoResumoResponse
{
    public long Id { get; set; }
    public string CodigoInterno { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
