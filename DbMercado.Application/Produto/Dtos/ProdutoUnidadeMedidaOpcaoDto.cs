namespace DbMercado.Application.Produto.Dtos;

/// <summary>
/// Uma opção de unidade de medida (chave + rótulo em <c>dbParametro</c>, produto/unidadeMedida).
/// </summary>
public sealed record ProdutoUnidadeMedidaOpcaoDto(string Codigo, string Rotulo);
