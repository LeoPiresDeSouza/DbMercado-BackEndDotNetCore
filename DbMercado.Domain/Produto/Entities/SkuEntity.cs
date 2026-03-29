using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.Entities;

/// <summary>
/// SKU (unidade de venda/estoque) pertencente ao agregado <see cref="ProdutoEntity"/>.
/// </summary>
public class SkuEntity : BaseEntity
{
    public long Id { get; private set; }

    public long ProdutoId { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public ProdutoEntity Produto { get; private set; } = null!;

    public SkuEntity()
    {
    }

    internal static SkuEntity CriarNovaEntrada(
        ProdutoEntity produto,
        string codigo,
        bool ativo,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(produto);
        if (string.IsNullOrWhiteSpace(codigo))
            throw new BusinessException("SKU_CODIGO_OBRIGATORIO", "Código do SKU é obrigatório.");

        var agora = DateTime.UtcNow;
        return new SkuEntity
        {
            Produto = produto,
            ProdutoId = produto.Id,
            Codigo = codigo.Trim(),
            Ativo = ativo,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }

    internal void AlterarAtivo(bool ativo, string usuarioAuditoria)
    {
        if (Ativo == ativo)
            return;

        Ativo = ativo;
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }
}
