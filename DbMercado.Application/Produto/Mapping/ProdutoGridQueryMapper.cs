using System.Text.Json;
using DbMercado.Application.Produto.Dtos;
using DbMercado.Domain.Produto.Queries;

namespace DbMercado.Application.Produto.Mapping;

public static class ProdutoGridQueryMapper
{
    private static readonly HashSet<string> CamposOrdenacao = new(StringComparer.OrdinalIgnoreCase)
    {
        "id",
        "nome",
        "marca",
        "unidadeMedida"
    };

    public static ProdutoGridSpecification ToSpecification(ProdutoGridQueryDto dto)
    {
        var skip = Math.Max(0, dto.StartRow);
        var rawTake = dto.EndRow - dto.StartRow;
        var take = rawTake <= 0 ? 100 : Math.Clamp(rawTake, 1, ProdutoGridSpecification.TamanhoMaximoPagina);

        var ordenacao = MapearOrdenacao(dto.SortModel);
        var filtro = MapearFiltros(dto.FilterModel);

        return new ProdutoGridSpecification
        {
            Skip = skip,
            Take = take,
            Ordenacao = ordenacao,
            Filtro = filtro
        };
    }

    private static IReadOnlyList<ProdutoGridOrdenacao> MapearOrdenacao(List<ProdutoGridSortItemDto>? sortModel)
    {
        if (sortModel is not { Count: > 0 })
            return Array.Empty<ProdutoGridOrdenacao>();

        var lista = new List<ProdutoGridOrdenacao>();
        foreach (var item in sortModel)
        {
            var campo = NormalizarCampoOrdenacao(item.ColId);
            if (campo is null)
                continue;

            var asc = !string.Equals(item.Sort, "desc", StringComparison.OrdinalIgnoreCase);
            lista.Add(new ProdutoGridOrdenacao { Campo = campo, Crescente = asc });
        }

        return lista;
    }

    private static string? NormalizarCampoOrdenacao(string colId)
    {
        if (string.IsNullOrWhiteSpace(colId))
            return null;

        var c = colId.Trim();
        return CamposOrdenacao.Contains(c) ? c : null;
    }

    private static ProdutoGridFiltro MapearFiltros(JsonElement? filterModel)
    {
        if (filterModel is not { ValueKind: JsonValueKind.Object } root)
            return new ProdutoGridFiltro();

        var id = new List<FiltroNumeroColuna>();
        var nome = new List<FiltroTextoColuna>();
        var marca = new List<FiltroTextoColuna>();
        var unidade = new List<FiltroTextoColuna>();

        foreach (var prop in root.EnumerateObject())
        {
            switch (prop.Name)
            {
                case "id":
                    id.AddRange(ExtrairFiltrosNumero(prop.Value));
                    break;
                case "nome":
                    nome.AddRange(ExtrairFiltrosTexto(prop.Value));
                    break;
                case "marca":
                    marca.AddRange(ExtrairFiltrosTexto(prop.Value));
                    break;
                case "unidadeMedida":
                    unidade.AddRange(ExtrairFiltrosTexto(prop.Value));
                    break;
            }
        }

        return new ProdutoGridFiltro
        {
            Id = id,
            Nome = nome,
            Marca = marca,
            UnidadeMedida = unidade
        };
    }

    private static IEnumerable<FiltroNumeroColuna> ExtrairFiltrosNumero(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object)
            yield break;

        if (TryGetStringProperty(el, "operator", out _) && el.TryGetProperty("conditions", out var conds)
            && conds.ValueKind == JsonValueKind.Array)
        {
            foreach (var c in conds.EnumerateArray())
            {
                var one = MapearFiltroNumeroSimples(c);
                if (one is not null)
                    yield return one;
            }

            yield break;
        }

        var single = MapearFiltroNumeroSimples(el);
        if (single is not null)
            yield return single;
    }

    private static FiltroNumeroColuna? MapearFiltroNumeroSimples(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object)
            return null;

        if (!TryGetStringProperty(el, "filterType", out var ft) || !string.Equals(ft, "number", StringComparison.OrdinalIgnoreCase))
            return null;

        if (!TryGetStringProperty(el, "type", out var type))
            return null;

        var op = type.ToLowerInvariant() switch
        {
            "equals" => NumeroFiltroOperador.Igual,
            "notEqual" => NumeroFiltroOperador.Diferente,
            "lessThan" => NumeroFiltroOperador.Menor,
            "lessThanOrEqual" => NumeroFiltroOperador.MenorOuIgual,
            "greaterThan" => NumeroFiltroOperador.Maior,
            "greaterThanOrEqual" => NumeroFiltroOperador.MaiorOuIgual,
            "inRange" => NumeroFiltroOperador.Entre,
            "blank" => NumeroFiltroOperador.EmBranco,
            "notBlank" => NumeroFiltroOperador.NaoEmBranco,
            _ => (NumeroFiltroOperador?)null
        };

        if (op is null)
            return null;

        long? v = null;
        long? v2 = null;
        if (el.TryGetProperty("filter", out var f) && TryReadLong(f, out var lv))
            v = lv;
        if (el.TryGetProperty("filterTo", out var f2) && TryReadLong(f2, out var lv2))
            v2 = lv2;

        return new FiltroNumeroColuna(op.Value, v, v2);
    }

    private static IEnumerable<FiltroTextoColuna> ExtrairFiltrosTexto(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object)
            yield break;

        if (TryGetStringProperty(el, "operator", out _) && el.TryGetProperty("conditions", out var conds)
            && conds.ValueKind == JsonValueKind.Array)
        {
            foreach (var c in conds.EnumerateArray())
            {
                var one = MapearFiltroTextoSimples(c);
                if (one is not null)
                    yield return one;
            }

            yield break;
        }

        var single = MapearFiltroTextoSimples(el);
        if (single is not null)
            yield return single;
    }

    private static FiltroTextoColuna? MapearFiltroTextoSimples(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object)
            return null;

        if (!TryGetStringProperty(el, "filterType", out var ft) || !string.Equals(ft, "text", StringComparison.OrdinalIgnoreCase))
            return null;

        if (!TryGetStringProperty(el, "type", out var type))
            return null;

        var op = type.ToLowerInvariant() switch
        {
            "contains" => TextoFiltroOperador.Contem,
            "notContains" => TextoFiltroOperador.NaoContem,
            "equals" => TextoFiltroOperador.Igual,
            "notEqual" => TextoFiltroOperador.Diferente,
            "startsWith" => TextoFiltroOperador.ComecaCom,
            "endsWith" => TextoFiltroOperador.TerminaCom,
            "blank" => TextoFiltroOperador.EmBranco,
            "notBlank" => TextoFiltroOperador.NaoEmBranco,
            _ => (TextoFiltroOperador?)null
        };

        if (op is null)
            return null;

        string? filtro = null;
        if (el.TryGetProperty("filter", out var fe))
        {
            if (fe.ValueKind == JsonValueKind.String)
                filtro = fe.GetString();
            else if (fe.ValueKind == JsonValueKind.Number && fe.TryGetInt64(out var n))
                filtro = n.ToString();
        }

        return new FiltroTextoColuna(op.Value, filtro);
    }

    private static bool TryGetStringProperty(JsonElement el, string name, out string value)
    {
        value = string.Empty;
        if (!el.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.String)
            return false;
        value = p.GetString() ?? string.Empty;
        return true;
    }

    private static bool TryReadLong(JsonElement el, out long value)
    {
        value = 0;
        return el.ValueKind switch
        {
            JsonValueKind.Number when el.TryGetInt64(out value) => true,
            JsonValueKind.String when long.TryParse(el.GetString(), out value) => true,
            _ => false
        };
    }
}
