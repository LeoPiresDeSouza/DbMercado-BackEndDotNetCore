using DbMercado.Infrastructure.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace DbMercado.Infrastructure.Shared.Data;

/// <summary>
/// Permite <c>dotnet ef</c> sem depender de credenciais no código-fonte (User Secrets / env / appsettings locais).
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "DbMercado.Api"));
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("d4f8b2a0-1c6e-4a9b-8e3d-7f2a5b9c1e4d")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Defina ConnectionStrings:DefaultConnection (User Secrets, variável ConnectionStrings__DefaultConnection ou appsettings locais não versionados).");
        }

        ApplicationSettings.DataBase.SetConnectionString(connectionString);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .ConfigureWarnings(w => w.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning))
            .Options;

        var encryption = new AesEncryptionService(configuration);
        return new AppDbContext(options, encryption);
    }
}
