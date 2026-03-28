using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Importacao.Dtos;

public class NotaFiscalItemCadastroRequest
{
    [Range(1, 9999)]
    public int NumeroItem { get; set; }

    [MaxLength(60)]
    public string? CodigoProdutoFornecedor { get; set; }

    [Required]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [MaxLength(8)]
    public string? Ncm { get; set; }

    public decimal Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal ValorTotalLinha { get; set; }
}
