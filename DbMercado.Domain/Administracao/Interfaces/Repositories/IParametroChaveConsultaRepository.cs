namespace DbMercado.Domain.Administracao.Interfaces.Repositories;

/// <summary>
/// Consulta se existe parâmetro com a chave informada (categoria + atributo + chave).
/// </summary>
public interface IParametroChaveConsultaRepository
{
    Task<bool> ExisteChaveAsync(
        string categoria,
        string atributo,
        string chave,
        CancellationToken cancellationToken = default);
}
