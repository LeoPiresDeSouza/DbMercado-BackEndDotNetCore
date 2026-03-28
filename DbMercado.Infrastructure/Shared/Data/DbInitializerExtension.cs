using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbMercado.Infrastructure.Shared.Data;


/// <summary>
/// Custom MiddleWare para inicialização do banco de dados da aplicação.
/// </summary>
/// <remarks>
/// O método estátivo DataBaseSeeder deve ser registrado como um middleware em Program.cs - app.DataBaseSeeder().
/// O serviço DbInitializer deve ser registrado como um serviço da aplicação em Program.cs - services.AddScoped<DbInitializer>();
/// </remarks>
public static class DbInitializerExtension
{
    public static IApplicationBuilder DataBaseSeeder(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
			var provider = services.GetRequiredService<IServiceProvider>();

			DbInitializer.Initialize(context, loggerFactory, provider);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"{DataBaseSeeder} - {ex.ToString()}");
            Console.WriteLine($"{DataBaseSeeder} - {ex.ToString()}");
        }

        return app;
    }
}
