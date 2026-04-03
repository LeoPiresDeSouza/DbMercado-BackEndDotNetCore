using DbMercado.Domain.Shared.Entities;
using DbMercado.Domain.Shared.Exceptions;
using System.Globalization;
using System.Text;

namespace DbMercado.Domain.Produto.Entities;

/// <summary>
/// Nó da árvore de categorias de produto (auto-referenciada, até 4 níveis).
/// Exemplo: Alimentos (1) → Bebidas (2) → Cervejas (3) → Lager (4).
/// </summary>
public class CategoriaProdutoEntity : BaseEntity
{
    public long Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>
    /// Identificador único para URLs e filtros (ex.: "bebidas-alcoolicas").
    /// Gerado automaticamente a partir do nome; pode ser sobrescrito.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    /// <summary>Nulo indica categoria raiz (nível 1).</summary>
    public long? CategoriaPaiId { get; private set; }

    public CategoriaProdutoEntity? CategoriaPai { get; private set; }

    public ICollection<CategoriaProdutoEntity> Subcategorias { get; private set; }
        = new List<CategoriaProdutoEntity>();

    /// <summary>Nível na hierarquia: 1 (raiz) a 4 (folha).</summary>
    public int Nivel { get; private set; }

    public bool Ativo { get; private set; } = true;

    /// <summary>Construtor para materialização pelo ORM.</summary>
    private CategoriaProdutoEntity() { }

    /// <summary>
    /// Cria uma categoria raiz (sem pai, nível 1).
    /// </summary>
    public static CategoriaProdutoEntity CriarRaiz(
        string nome,
        string? descricao,
        string usuarioAuditoria)
    {
        ValidarNome(nome);

        var agora = DateTime.UtcNow;
        return new CategoriaProdutoEntity
        {
            Nome = nome.Trim(),
            Slug = GerarSlug(nome),
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            CategoriaPaiId = null,
            Nivel = 1,
            Ativo = true,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }

    /// <summary>
    /// Cria uma subcategoria filha de outra categoria.
    /// </summary>
    public static CategoriaProdutoEntity CriarFilha(
        CategoriaProdutoEntity pai,
        string nome,
        string? descricao,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(pai);
        ValidarNome(nome);

        var nivelFilha = pai.Nivel + 1;
        if (nivelFilha > 4)
            throw new BusinessException(
                "CATEGORIA_NIVEL_MAXIMO",
                "A hierarquia de categorias suporta no máximo 4 níveis.");

        var agora = DateTime.UtcNow;
        return new CategoriaProdutoEntity
        {
            Nome = nome.Trim(),
            Slug = GerarSlug(nome),
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            CategoriaPaiId = pai.Id,
            CategoriaPai = pai,
            Nivel = nivelFilha,
            Ativo = true,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }

    public void Atualizar(string nome, string? descricao, string usuarioAuditoria)
    {
        ValidarNome(nome);
        Nome = nome.Trim();
        Slug = GerarSlug(nome);
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    public void Ativar(string usuarioAuditoria)
    {
        Ativo = true;
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    public void Inativar(string usuarioAuditoria)
    {
        Ativo = false;
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessException(
                "CATEGORIA_NOME_OBRIGATORIO",
                "Nome da categoria é obrigatório.");

        if (nome.Trim().Length > 128)
            throw new BusinessException(
                "CATEGORIA_NOME_MUITO_LONGO",
                "Nome da categoria deve ter no máximo 128 caracteres.");
    }

    /// <summary>
    /// Gera um slug a partir do nome: minúsculas, sem acentos, hífens no lugar de espaços.
    /// Exemplo: "Bebidas Alcoólicas" → "bebidas-alcoolicas"
    /// </summary>
    public static string GerarSlug(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return string.Empty;

        var normalizado = nome.Trim().ToLowerInvariant();

        var semAcento = new StringBuilder();
        foreach (var ch in normalizado.Normalize(NormalizationForm.FormD))
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (categoria != UnicodeCategory.NonSpacingMark)
                semAcento.Append(ch);
        }

        var slug = semAcento.ToString()
            .Replace(' ', '-')
            .Replace("--", "-");

        var resultado = new StringBuilder();
        foreach (var ch in slug)
        {
            if (char.IsLetterOrDigit(ch) || ch == '-')
                resultado.Append(ch);
        }

        return resultado.ToString().Trim('-');
    }
}
