using DbMercado.Api.Extensions;
using DbMercado.Api.Hubs;
using DbMercado.Api.Middlewares;
using DbMercado.Api.Workers;
using DbMercado.Api.RealTime;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Application.Administracao.Services;
using DbMercado.Application.Importacao.Interfaces;
using DbMercado.Application.Importacao.Services;
using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.Application.Chat.Services;
using DbMercado.Application.Produto.Interfaces;
using DbMercado.Application.Produto.Services;
using DbMercado.Infrastructure.Produto.Services;
using DbMercado.Domain.Shared;
using DbMercado.Domain.Shared.Interfaces.Repositories;
using DbMercado.CrossCutting.Settings;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;
using DbMercado.Domain.Administracao.Interfaces.Services.Autenticacao;
using DbMercado.Domain.Administracao.Interfaces.UnitsOfWork;
using DbMercado.Domain.Importacao.Interfaces.UnitsOfWork;
using DbMercado.Domain.Produto.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Administracao.Repositories;
using DbMercado.Infrastructure.Chat.Caching;
using DbMercado.Infrastructure.Chat.RateLimiting;
using DbMercado.Infrastructure.Chat.Translation;
using DbMercado.Infrastructure.Chat.UnitsOfWork;
using DbMercado.Infrastructure.Administracao.Services;
using DbMercado.Infrastructure.Administracao.Services.Autenticacao;
using DbMercado.Infrastructure.Administracao.UnitsOfWork;
using DbMercado.Infrastructure.Importacao.UnitsOfWork;
using DbMercado.Infrastructure.Produto.UnitsOfWork;
using DbMercado.Infrastructure.Shared.Repositories;
using DbMercado.Infrastructure.Providers.Logging;
using DbMercado.Infrastructure.Providers.CEP;
using DbMercado.Application.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Security;
using DbMercado.Infrastructure.Shared.HTTP;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Jobs.DependencyInjection;
using DeepBlues.Infrastructure.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using DbMercado.Api.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.IO;
using System.Reflection;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection não configurada. Use User Secrets, variável ConnectionStrings__DefaultConnection ou appsettings locais não versionados.");
}

ApplicationSettings.DataBase.SetConnectionString(connectionString);

#region Banco de Dados (Infrastructure)

builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.ConfigureWarnings(w => w.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
    options.UseSqlServer(connectionString);
});

builder.Services.AddDbMercadoQuartz(builder.Configuration);

#endregion Banco de Dados (Infrastructure)




#region Identity Configuration (Infrastructure)

    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredUniqueChars = 0;
        options.Lockout.AllowedForNewUsers = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

#endregion Identity Configuration (Infrastructure)




#region Http Client com Polly

builder.Services.AddHttpClient("ViaCep", client =>
{
    client.BaseAddress = new Uri("https://viacep.com.br/ws/");
})
.AddPolicyHandler(HttpPolicies.RetryPolicy())
.AddPolicyHandler(HttpPolicies.CircuitBreakerPolicy());

builder.Services.AddHttpClient("BrasilApi", client =>
{
    client.BaseAddress = new Uri("https://brasilapi.com.br/api/cep/v1/");
})
.AddPolicyHandler(HttpPolicies.RetryPolicy())
.AddPolicyHandler(HttpPolicies.CircuitBreakerPolicy());

builder.Services.Configure<OpenRouterOptions>(
    builder.Configuration.GetSection(OpenRouterOptions.SectionName));

builder.Services.AddHttpClient(OpenRouterTranslationService.HttpClientName, client =>
{
    client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
    client.Timeout = TimeSpan.FromMinutes(2);
})
.AddPolicyHandler((sp, _) =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    var log = lf.CreateLogger("DbMercado.OpenRouter.HttpRetry");
    return HttpPolicies.OpenRouterTranslationRetryPolicy(log);
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("cepLimiter", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 20;
        opt.QueueLimit = 5;
    });
});

#endregion Http Client com Polly




#region Autenticação JWT

// ---------------------------------------------------------------------------
// DESENVOLVIMENTO: fallback embutido só roda com ASPNETCORE_ENVIRONMENT=Development.
// Antes de produção: remova o PostConfigure e o if que preenche Secret e exija sempre
// JwtSettings__Secret / user-secrets / Key Vault (nunca commitar segredo real).
// ---------------------------------------------------------------------------
const string JwtSecretDevelopmentFallback =
    "DbMercado-DEV-JWT-CHANGE-BEFORE-PRODUCTION-USE-ENV-SECRETS!!";

var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.Configure<LogBackupStorageOptions>(
    builder.Configuration.GetSection(LogBackupStorageOptions.SectionName));
builder.Services.PostConfigure<JwtSettings>(opts =>
{
    if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(opts.Secret))
    {
        opts.Secret = JwtSecretDevelopmentFallback;
    }
});

var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.Secret) && builder.Environment.IsDevelopment())
{
    jwtSettings.Secret = JwtSecretDevelopmentFallback;
}

if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret não configurado. Defina variável JwtSettings__Secret, user-secrets ou appsettings seguros. " +
        "O fallback embutido existe apenas em Development.");
}

var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Em prod, mude para true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        // SignalR WebSocket: o cliente envia JWT em access_token na query (header Authorization não acompanha o upgrade).
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments(ChatHubRoute.Path))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

#endregion Autenticação JWT




#region Add services to the container.

builder.Services.AddAuthorization(options =>
{
    // Pol?ticas usadas pelo TestController e por outros endpoints protegidos
    options.AddPolicy("CanRead", policy => policy.RequireClaim("NivelAcesso"));
    options.AddPolicy("CanViewLogs", policy => policy.RequireClaim("NivelAcesso", ((int)DbMercado.Domain.Shared.ApplicationSettings.ClaimApp.NivelAcesso.Administrador).ToString()));
    options.AddPolicy("CanManageMenus", policy => policy.RequireClaim("NivelAcesso", ((int)DbMercado.Domain.Shared.ApplicationSettings.ClaimApp.NivelAcesso.Administrador).ToString()));

    options.AddPolicy(ChatMultilingueAcessarRequirement.PolicyName, policy =>
        policy.Requirements.Add(new ChatMultilingueAcessarRequirement()));
});

// Com fetch credentials: 'include' no React, não pode usar AllowAnyOrigin (*).
// Em Development: qualquer origem refletida + credenciais (evita falha ao abrir pelo IP da LAN).
// Fora disso: apenas localhost / 127.0.0.1 (ajuste com origens reais em produção).
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    return !string.IsNullOrEmpty(origin);
                }

                if (string.IsNullOrEmpty(origin))
                {
                    return false;
                }

                try
                {
                    var uri = new Uri(origin);
                    if (uri.Scheme is not ("http" or "https"))
                    {
                        return false;
                    }

                    return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                        || uri.Host == "127.0.0.1";
                }
                catch (UriFormatException)
                {
                    return false;
                }
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IUserIdProvider, ChatUserIdProvider>();
builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
builder.Services.AddScoped<IChatConviteRealtimeNotifier, ChatConviteSignalRNotifier>();

#endregion Add services to the container.




#region Serviços de middlewares customizados

// Registra o serviço de inicialização da aplicação.
// O Middleware que irá executar o serviço deve ser registrado posteriormente.
// app.CustomApplicationInitializer();

builder.Services.AddScoped<DbInitializer>();

#endregion Serviços de middlewares customizados




#region Swagger

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minha API .NET 9", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.TagActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues.TryGetValue("controller", out var cn)
            ? cn
            : "API";
        return controller switch
        {
            "ChatRooms" or "ChatInvites" or "ChatMembers" => new[] { "Chat multilíngue" },
            _ => new[] { controller! }
        };
    });

    // Define o esquema de seguran?a (Bearer Token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Cabeçalho de autorização JWT usando o esquema Bearer.
                      Escreva 'Bearer' [espaço] e então seu token.
                      Exemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

#endregion Swagger




#region Injeção de dependância da UnitsOfWork

builder.Services.AddScoped<IUwAdministracao, UwAdministracao>();
builder.Services.AddScoped<IUwImportacao, UwImportacao>();
builder.Services.AddScoped<IUwProduto, UwProduto>();
builder.Services.AddScoped<IUwChat, UwChat>();

#endregion Injeção de dependância de repositários




#region Injeção de dependância de repositórios

builder.Services.AddScoped<IApplicationCachingFactory, ApplicationCachingFactory> ();

// Registro o serviço de cache genérico aberto
// Isso diz ao .NET: "Sempre que alguém pedir IApplicationCachingService<T>,
// use a classe ApplicationCachingService<T>"

builder.Services.AddTransient(typeof(IApplicationCachingService<>), typeof(ApplicationCachingService<>));
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
builder.Services.AddScoped<IParametroChaveConsultaRepository, ParametroChaveConsultaRepository>();
builder.Services.AddScoped<IAppLogRepository, AppLogRepository>();
builder.Services.AddScoped<IJobExecucaoRepository, JobExecucaoRepository>();

#endregion Injeção de dependância de repositários




#region Injeção de dependência de serviços

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();
builder.Services.AddSingleton<DatabaseLoggerProvider>();
builder.Services.AddScoped<IModuloService, ModuloService>();

builder.Services.AddScoped<ICepProvider, ViaCepService>();
builder.Services.AddScoped<ICepProvider, BrasilApiService>();

builder.Services.AddScoped<CepFallbackCepService>();

builder.Services.AddScoped<ICepProvider>(sp =>
{
    var providers = sp.GetServices<ICepProvider>();
    return new CepFallbackCepService(providers);
});

builder.Services.AddScoped<ICepService, CepService>();
builder.Services.AddScoped<IImportacaoService, ImportacaoService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<ICategoriaProdutoService, CategoriaProdutoService>();
builder.Services.AddScoped<IMidiaArquivoStorage, MidiaArquivoStorage>();
builder.Services.AddScoped<IMidiaService, MidiaService>();

builder.Services.AddScoped<IPermissaoUsuarioResolver, PermissaoUsuarioResolver>();
builder.Services.AddScoped<IAuthorizationHandler, ChatMultilingueAcessarHandler>();
builder.Services.AddScoped<IAppLogBackupStoragePaths, AppLogBackupStoragePaths>();
builder.Services.AddScoped<IAppLogService, AppLogService>();
builder.Services.AddScoped<IJobExecucaoAdministracaoService, JobExecucaoAdministracaoService>();
builder.Services.AddScoped<IChatSalaService, ChatSalaService>();
builder.Services.AddScoped<IChatConviteService, ChatConviteService>();
builder.Services.AddScoped<IChatMembroService, ChatMembroService>();
builder.Services.AddScoped<IChatMensagemService, ChatMensagemService>();
builder.Services.AddSingleton<IChatMensagemEnvioRateLimiter, ChatMensagemEnvioRateLimiter>();
builder.Services.AddSingleton<ITranslationCache, InMemoryTranslationCache>();
builder.Services.AddSingleton<IOpenRouterTranslationService, OpenRouterTranslationService>();

var translationJobsChannel = Channel.CreateBounded<TranslationJob>(
    new BoundedChannelOptions(5000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });
builder.Services.AddSingleton(translationJobsChannel);
builder.Services.AddSingleton(translationJobsChannel.Reader);
builder.Services.AddSingleton(translationJobsChannel.Writer);
builder.Services.AddHostedService<TranslationWorker>();

#endregion Injeção de dependência de serviços


var app = builder.Build();

app.UseDatabaseLogger();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();   // Gera o JSON do Swagger
    app.UseSwaggerUI(); // Gera a interface gráfica HTML
//}

/*
    Registra o middleware de Inicialização da aplicação, definido em Infrastructure.Data.ApplicationInitializer.
    O serviço a ser injetado no middleware para executar a inicialização do sistema foi previamente registrado
    builder.Services.AddScoped<AppInitializer>();
*/

app.DataBaseSeeder();

// Tratamento global de exceções (ProblemDetails) e scope de log — antes de auth
app.UseMiddleware<ErrorHandlingMiddleware>();

// Em Development com perfil só HTTP (ex.: :5046), não há porta HTTPS — redirect gera warning e não ajuda o SPA.
if (!app.Environment.IsDevelopment() || app.Configuration.GetValue("UseHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseCors(); // Adicionado para permitir requisições de outras origens
app.UseStaticFiles();
app.UseAuthentication();  // Deve vir antes de UseAuthorization para validar o JWT
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>(ChatHubRoute.Path);

if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
            var baseUrl = addresses is { Count: > 0 }
                ? addresses.FirstOrDefault(static a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                  ?? addresses.First()
                : "http://localhost:5046";
            var swaggerUrl = $"{baseUrl.TrimEnd('/')}/swagger";
            Process.Start(new ProcessStartInfo { FileName = swaggerUrl, UseShellExecute = true });
        }
        catch
        {
            // Sem navegador ou endereço ainda indisponível — ignorar em dev.
        }
    });
}

app.Run();
