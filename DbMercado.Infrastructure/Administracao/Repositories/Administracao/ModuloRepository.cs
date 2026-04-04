using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DeepBlues.Infrastructure.Providers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DbMercado.Infrastructure.Administracao.Repositories.Administracao;

public class ModuloRepository: BaseRepository<ModuloEntity>, IModuloRepository
{
    #region Membros Privados

    private readonly IApplicationCachingService<ModuloEntity> _cache;

    #endregion Membros Privados




    #region Propriedades públicas
    #endregion Propriedades públicas





    #region Ctor

    public ModuloRepository(AppDbContext context,
                            IApplicationCachingService<ModuloEntity> cache) : base(context)
    {
        _cache = cache;
    }

    #endregion Ctor




    #region Métodos públicos

    /// <summary>
    /// Retorna a lista de módulos associados a um usuário específico,
    /// juntamente com as funcionalidades correspondentes a cada módulo e as permissões
    /// associadas a cvada funcionalidade.
    /// Essa consulta é essencial para determinar quais módulos e funcionalidades um usuário tem acesso,
    /// permitindo uma gestão eficiente de permissões e personalização da interface de acordo com o perfil do usuário.
    /// </summary>
    /// <param name="usuario">nome do usuário no identity</param>
    /// <param name="permissoesIds">Lista com os longs representando os ids das permissões de acesso associadas ao usuário</param>
    /// <returns>List<ModuloEntity></returns>
    public async Task<List<ModuloEntity>> ModulosUsuarioAsync(string usuario, List<long> permissoesIds)
    {
        // A lista de módulos depende das permissões; chave só por usuário retornava lista errada/stale
        // (ex.: primeiro hit sem permissão de produtos cacheava vazio e o menu lateral sumia para sempre).
        var permFingerprint = permissoesIds.Count == 0
            ? "0"
            : string.Join('-', permissoesIds.OrderBy(id => id));
        var cacheKey = $"ModulosUsuarioAsync:{usuario}:{permFingerprint}";

        var modulo = await _cache.GetOrCreateAsync(
                           CachePolicy.MediumTerm,
                           cacheKey,
                           () => getModulosUsuarioAsync(usuario, permissoesIds));

        return modulo;
    }

    #endregion Métodos públicos




    #region Métodos privados

    public async Task<List<ModuloEntity>> getModulosUsuarioAsync(string usuario, List<long> permissoesIds)
    {
        //var permissoesIds = new List<long>();
        //permissoesIds.AddRange(1, 2, 3, 4, 5);

        // Recupera a estrutura de acesso do usuário aos módulos, funcionalidades e permissões.
        // O json é utilizado para montar o menu de acesso do usuário no frontend.

        var permissoes = await _context.Permissoes
            .AsNoTracking()
            .Where(p => permissoesIds.Contains(p.Id))
            .Select(p => new
            {
                Permissao = p,
                Funcionalidade = p.Funcionalidade,
                Modulo = p.Funcionalidade.Modulo
            })
            .ToListAsync();

        var modulos = permissoes
            .GroupBy(x => x.Modulo.Id)
            .Select(moduloGroup =>
            {
                var modulo = moduloGroup.First().Modulo;

                modulo.Funcionalidades = moduloGroup
                    .GroupBy(x => x.Funcionalidade.Id)
                    .Select(funcGroup =>
                    {
                        var funcionalidade = funcGroup.First().Funcionalidade;

                        funcionalidade.Permissoes = funcGroup
                            .Select(x => x.Permissao)
                            .ToList();

                        return funcionalidade;
                    })
                    .OrderBy(f => f.OrdemExibicao)
                    .ToList();

                return modulo;
            })
            .OrderBy(m => m.OrdemExibicao)
            .ToList();

        return modulos;

    }

    #endregion Métodos privados
}
