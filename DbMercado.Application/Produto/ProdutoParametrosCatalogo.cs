namespace DbMercado.Application.Produto;

/// <summary>
/// Convenção de <see cref="DbMercado.Domain.Administracao.Entities.ParametroEntity"/>
/// para o domínio de produto. Valores alinhados ao seed em <c>DbInitializer</c>.
/// </summary>
public static class ProdutoParametrosCatalogo
{
    public const string Categoria = "produto";

    /// <summary>Como o produto é vendido/faturado (ex.: UN, KIT, DZ, CX).</summary>
    public const string AtributoUnidadeComercializacao = "unidadeComercializacao";

    /// <summary>Natureza física do produto para NF-e (ex.: UN, KG, L, M).</summary>
    public const string AtributoUnidadeMedida = "unidadeMedida";

    /// <summary>Tipo de acondicionamento/embalagem (ex.: CX, FD, PCT, LAT).</summary>
    public const string AtributoUnidadeEmbalagem = "unidadeEmbalagem";

    /// <summary>Unidade das dimensões lineares — altura, largura, comprimento (ex.: CM, M, MM).</summary>
    public const string AtributoUnidadeDimensao = "unidadeDimensao";

    /// <summary>Unidade de massa logística — peso bruto (ex.: KG, G, T).</summary>
    public const string AtributoUnidadePeso = "unidadePeso";

    /// <summary>Origem geográfica (ex.: NACIONAL, IMPORTADO).</summary>
    public const string AtributoOrigemGeografica = "origemGeografica";

    /// <summary>Código de origem da mercadoria para ICMS — tabela SEFAZ (ex.: 0, 1, 2).</summary>
    public const string AtributoOrigemIcms = "origemIcms";
}
