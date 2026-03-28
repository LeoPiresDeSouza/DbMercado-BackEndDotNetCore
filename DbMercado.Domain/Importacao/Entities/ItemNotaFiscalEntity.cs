namespace DbMercado.Domain.Importacao.Entities;

/// <summary>
/// Item de linha da nota fiscal (produto/serviço documentado na NF).
/// </summary>
public class ItemNotaFiscalEntity : BaseEntity
{
    public long Id { get; set; }

    public long NotaFiscalId { get; set; }

    public NotaFiscalEntity NotaFiscal { get; set; } = null!;

    public int NumeroItem { get; set; }

    public string? CodigoProdutoFornecedor { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public string? Ncm { get; set; }

    public decimal Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal ValorTotalLinha { get; set; }
}
