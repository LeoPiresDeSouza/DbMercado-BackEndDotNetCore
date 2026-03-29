using System.Text.Json;

namespace DbMercado.Application.Produto.Dtos;

/// <summary>
/// Corpo da consulta alinhado ao <c>IGetRowsParams</c> do AG Grid (infinite row model).
/// </summary>
public class ProdutoGridQueryDto
{
    public int StartRow { get; set; }

    public int EndRow { get; set; }

    public List<ProdutoGridSortItemDto>? SortModel { get; set; }

    /// <summary>Objeto <c>filterModel</c> serializado (chaves = colId).</summary>
    public JsonElement? FilterModel { get; set; }

    /// <summary>Metadados SSRM: colunas arrastadas para linhas de grupo.</summary>
    public List<ProdutoGridColumnVoDto>? RowGroupCols { get; set; }

    /// <summary>Metadados SSRM: caminho de chaves do grupo expandido.</summary>
    public List<string>? GroupKeys { get; set; }

    /// <summary>Metadados SSRM: colunas de agregação (painel Valores).</summary>
    public List<ProdutoGridColumnVoDto>? ValueCols { get; set; }

    /// <summary>Metadados SSRM (pivot completo não é suportado nesta API).</summary>
    public bool PivotMode { get; set; }
}

public class ProdutoGridSortItemDto
{
    public string ColId { get; set; } = string.Empty;

    /// <summary>Valores típicos: asc, desc.</summary>
    public string? Sort { get; set; }
}
