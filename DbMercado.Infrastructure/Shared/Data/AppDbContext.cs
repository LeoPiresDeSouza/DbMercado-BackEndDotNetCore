using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Importacao.Entities;
using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Shared.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using static DbMercado.Domain.Shared.ApplicationSettings;

namespace DbMercado.Infrastructure.Shared.Data;

public class AppDbContext: IdentityDbContext<IdentityUser>
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppLogEntity> AppLogEntries => Set<AppLogEntity>();
    public DbSet<ModuloEntity> Modulos => Set<ModuloEntity>();
    public DbSet<FuncionalidadeEntity> Funcionalidades=> Set<FuncionalidadeEntity>();
    public DbSet<PermissaoEntity> Permissoes => Set<PermissaoEntity>();
    public DbSet<PessoaEmailEntity> PessoaEmails=> Set<PessoaEmailEntity>();
    public DbSet<PessoaEnderecoEntity> PessoaEnderecos=> Set<PessoaEnderecoEntity>();
    public DbSet<PessoaEntity> Pessoas => Set<PessoaEntity>();
    public DbSet<PessoaFisicaEntity> PessoasFisicas=> Set<PessoaFisicaEntity>();
    public DbSet<PessoaJuridicaEntity> PessoasJuridicas=> Set<PessoaJuridicaEntity>();
    public DbSet<PessoaRedeSocialEntity> PessoaRedesSociais=> Set<PessoaRedeSocialEntity>();
    public DbSet<PessoaTelefoneEntity> PessoaTelefones=> Set<PessoaTelefoneEntity>();
    public DbSet<UsuarioEntity> Usuarios=> Set<UsuarioEntity>();
    public DbSet<ParametroEntity> Parametros => Set<ParametroEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    public DbSet<NotaFiscalEntity> NotasFiscais => Set<NotaFiscalEntity>();
    public DbSet<ItemNotaFiscalEntity> ItensNotaFiscal => Set<ItemNotaFiscalEntity>();
    public DbSet<ProdutoImportadoEntity> ProdutosImportados => Set<ProdutoImportadoEntity>();

    public DbSet<ProdutoEntity> Produtos => Set<ProdutoEntity>();
    public DbSet<SkuEntity> Skus => Set<SkuEntity>();
    public DbSet<CategoriaProdutoEntity> CategoriasProduto => Set<CategoriaProdutoEntity>();
    public DbSet<MidiaEntity> MidiasProduto => Set<MidiaEntity>();

    



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        if (!optionsBuilder.IsConfigured)
        {
            // Options incluída para evitar a mensagem - warn: Microsoft.EntityFrameworkCore.Query[10102]  Query uses a row limiting operation(Skip/ Take) without OrderBy, which may lead to unpredictable results.
            // torrando o saco no log da aplicação quando uma operação no entity framework com SingleOrDefault sem orderby.

            optionsBuilder.ConfigureWarnings(w => w.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
            optionsBuilder.UseSqlServer(DataBase.ConnectionString);
        }

        base.OnConfiguring(optionsBuilder);
    }

    // Essa função faz toda a vinculação mais complexa que não seja tão facil de configurar pelas classes, utilizando
    // o conceito de fluent api
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        ConfigureOptimisticConcurrency(modelBuilder);
    }

    /// <summary>
    /// Concorrência otimista (<c>rowversion</c>) nas entidades de negócio indicadas; demais herdeiras de <see cref="BaseEntity"/> ignoram <see cref="BaseEntity.RowVersion"/> no modelo.
    /// </summary>
    private static void ConfigureOptimisticConcurrency(ModelBuilder modelBuilder)
    {
        var concurrencyTypes = new HashSet<Type>
        {
            typeof(PessoaEntity),
            typeof(UsuarioEntity),
            typeof(ModuloEntity),
            typeof(ParametroEntity)
        };

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null || clrType.IsAbstract)
                continue;
            if (!typeof(BaseEntity).IsAssignableFrom(clrType))
                continue;

            var entityBuilder = modelBuilder.Entity(clrType);
            if (concurrencyTypes.Contains(clrType))
            {
                entityBuilder
                    .Property<byte[]>(nameof(BaseEntity.RowVersion))
                    .IsRowVersion()
                    .IsConcurrencyToken();
            }
            else
            {
                entityBuilder.Ignore(nameof(BaseEntity.RowVersion));
            }
        }
    }
}
