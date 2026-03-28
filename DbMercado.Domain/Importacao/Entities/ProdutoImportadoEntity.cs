namespace DbMercado.Domain.Importacao.Entities;

/// <summary>
/// Produto originado ou registrado a partir do processo de importação (cadastro mestre).
/// Pode referenciar o item de NF que deu origem ao cadastro.
/// </summary>
public class ProdutoImportadoEntity : BaseEntity
{
    public long Id { get; set; }

    /// <summary>Código interno único no sistema.</summary>
    public string CodigoInterno { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string? Ncm { get; set; }

    public string? UnidadeMedida { get; set; }

    public long? ItemNotaFiscalOrigemId { get; set; }

    public ItemNotaFiscalEntity? ItemNotaFiscalOrigem { get; set; }
}
