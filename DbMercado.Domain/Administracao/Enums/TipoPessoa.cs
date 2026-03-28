namespace DbMercado.Domain.Administracao.Enums;

/// <summary>
/// Classificação da pessoa no cadastro (persistida em coluna <c>TipoPessoa</c>).
/// </summary>
public enum TipoPessoa
{
    Indefinido = 0,
    Fisica = 1,
    Juridica = 2
}
