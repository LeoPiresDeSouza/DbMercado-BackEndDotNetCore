namespace DbMercado.Domain.Importacao.Entities;

/// <summary>
/// Nota fiscal de entrada (documento fiscal eletrônico ou registro equivalente).
/// Agregado com <see cref="ItemNotaFiscalEntity"/>.
/// </summary>
public class NotaFiscalEntity : BaseEntity
{
    public long Id { get; set; }

    /// <summary>Chave de acesso da NF-e (44 caracteres), quando aplicável.</summary>
    public string ChaveAcesso { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string Serie { get; set; } = string.Empty;

    public DateTimeOffset DataEmissao { get; set; }

    public string CnpjEmitente { get; set; } = string.Empty;

    public string? RazaoSocialEmitente { get; set; }

    public decimal ValorTotal { get; set; }

    public ICollection<ItemNotaFiscalEntity> Itens { get; set; } = new List<ItemNotaFiscalEntity>();
}
