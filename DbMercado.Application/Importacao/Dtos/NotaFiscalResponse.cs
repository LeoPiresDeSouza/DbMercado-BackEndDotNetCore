namespace DbMercado.Application.Importacao.Dtos;

public class NotaFiscalResponse
{
    public long Id { get; set; }
    public string ChaveAcesso { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public DateTimeOffset DataEmissao { get; set; }
    public string CnpjEmitente { get; set; } = string.Empty;
    public string? RazaoSocialEmitente { get; set; }
    public decimal ValorTotal { get; set; }
    public IReadOnlyList<ItemNotaFiscalResponse> Itens { get; set; } = Array.Empty<ItemNotaFiscalResponse>();
}

public class ItemNotaFiscalResponse
{
    public long Id { get; set; }
    public int NumeroItem { get; set; }
    public string? CodigoProdutoFornecedor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Ncm { get; set; }
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotalLinha { get; set; }
}

public class NotaFiscalResumoResponse
{
    public long Id { get; set; }
    public string ChaveAcesso { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public DateTimeOffset DataEmissao { get; set; }
    public decimal ValorTotal { get; set; }
}
