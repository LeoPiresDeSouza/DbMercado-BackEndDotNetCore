using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Importacao.Dtos;

public class ProdutoImportadoCadastroRequest
{
    [Required]
    [MaxLength(64)]
    public string CodigoInterno { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [MaxLength(8)]
    public string? Ncm { get; set; }

    [MaxLength(16)]
    public string? UnidadeMedida { get; set; }

    public long? ItemNotaFiscalOrigemId { get; set; }
}
