using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DbMercado.Infrastructure.Jobs.Configuration;

/// <summary>
/// Lê o valor de cron na tabela <c>dbParametro</c> durante a configuração do Quartz (startup).
/// Em falha de conexão ou linha ausente, retorna <c>null</c> para permitir fallback ao appsettings.
/// </summary>
public static class QuartzCronParametroLeitor
{
    public static string? ObterValorCron(
        IConfiguration configuration,
        string categoria,
        string atributo,
        string chave)
    {
        var cs = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(cs))
            return null;

        try
        {
            using var conn = new SqlConnection(cs);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                SELECT TOP (1) [Valor]
                FROM [dbParametro]
                WHERE [Categoria] = @categoria AND [Atributo] = @atributo AND [Chave] = @chave
                """;
            cmd.Parameters.AddWithValue("@categoria", categoria);
            cmd.Parameters.AddWithValue("@atributo", atributo);
            cmd.Parameters.AddWithValue("@chave", chave);

            var scalar = cmd.ExecuteScalar();
            var t = scalar?.ToString()?.Trim();
            return string.IsNullOrEmpty(t) ? null : t;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
