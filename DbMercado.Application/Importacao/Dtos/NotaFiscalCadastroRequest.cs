using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Importacao.Dtos;

public class NotaFiscalCadastroRequest
{
    [Required]
    [StringLength(44, MinimumLength = 44)]
    public string ChaveAcesso { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Serie { get; set; } = string.Empty;

    public DateTimeOffset DataEmissao { get; set; }

    [Required]
    [StringLength(14, MinimumLength = 14)]
    public string CnpjEmitente { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? RazaoSocialEmitente { get; set; }

    public decimal ValorTotal { get; set; }

    [Required]
    [MinLength(1)]
    public List<NotaFiscalItemCadastroRequest> Itens { get; set; } = new();
}
