using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Globalization;
using Microsoft.Extensions.Logging;
using DbMercado.Infrastructure.Shared.Data;

namespace DbMercado.Infrastructure.Shared.Repositories;

#pragma warning disable

public abstract class BaseRepository<T> where T : class
{

    #region Membros privados

    protected AppDbContext _context;    
    protected ILogger _logger;

    private DbSet<T> _dataSet;

    #endregion Membros privados




    #region Propriedades públicas

    /// <summary>
    /// Retorna o contexto que foi injetado na classe base Repositorio pelo construtor.
    /// </summary>
    /// 
    public AppDbContext Context
    {
        get { return _context; }
        set
        {
            _context = value;
            _dataSet = _context.Set<T>();
        }
    }


    /// <summary>
    /// Retorna o DbSet para acesso ao EntityFramework pela entidade.
    /// </summary>
    /// 
    public DbSet<T> DbSet => _dataSet;


    #endregion Propriedades públicas




    #region Ctor

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dataSet = _context.Set<T>();
    }

    #endregion Ctor




    #region Métodos públicos de Consulta

    /// <summary>
    /// Verifica se uma entrada com a chave primária fornecida existe na coleção da entidade.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>bool</returns>
    public async Task<bool> ExistsAsync(long id)
    {
        T result = await GetByIdAsync(id);
        return result == null ? false : true;
    }



    /// <summary>
    /// Conta a quantidade total de registros na entidade.
    /// </summary>
    /// <returns>int</returns>
    /// 
    public async Task<int> CountAsync()
    {
        return await _dataSet.CountAsync();
    }



    /// <summary>
    /// Conta a quantidade total de registros na entidade de acordo com o filtro especificado.
    /// </summary>
    /// <remarks>
    /// Utilização:
    /// var produtos = await _produtoRepository.CountAsync(p => p.Preco >= 50 && p.Preco <= 200);
    /// var produtosComNome = await _produtoRepository.CountAsync(p => p.Nome == "Notebook");
    /// </remarks>
    /// <returns>int.</returns>
    /// 
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dataSet.CountAsync(predicate);
    }



    /// <summary>
    /// <c>GetAllAsync()</c> retorna uma entida inteira do Entity Framework.
    /// Esse método está marcado como virtual para aceitar override nas classe que implementam
    /// o repositório, de forma a poderem acrescentar .Include de entidades associadas.
    /// </summary>
    /// <returns>List<T></returns>
    /// 
    public virtual async Task<List<T>> GetAllAsync()
    {
        List<T> result = await _dataSet.ToListAsync();
        return result;
    }



    /// <summary>
    /// Retorna a entidade identificada pela sua chave primária.
    /// </summary>
    /// <param name="id">Chave primária.</param>
    /// <returns>T</returns>
    /// 
    public virtual async Task<T> GetByIdAsync(long id)
    {
        var result = await _dataSet.FindAsync(id);
        return result;
    }



    /// <summary>
    /// Busca entidades que atendam a uma condição específica, usando uma expressão lambda.
    /// </summary>
    /// <remarks>
    /// Utilização:
    /// var produtos = await _produtoRepository.FindAsync(p => p.Preco >= 50 && p.Preco <= 200);
    /// var produtosComNome = await _produtoRepository.FindAsync(p => p.Nome == "Notebook");
    /// </remarks>
    /// <param name="predicate"></param>
    /// <returns>IEnumerable<T></returns>
    public async Task<IEnumerable<T>> FindCollectionAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dataSet.Where(predicate).ToListAsync();
    }



    /// <summary>
    /// Busca uma entidade que atenda a uma condição específica, usando uma expressão lambda.
    /// </summary>
    /// <remarks>
    /// Utilização:
    /// var produtos = await _produtoRepository.FindAsync(p => p.Preco >= 50 && p.Preco <= 200);
    /// var produtosComNome = await _produtoRepository.FindAsync(p => p.Nome == "Notebook");
    /// </remarks>
    /// <param name="predicate"></param>
    /// <returns>T</returns>
    public async Task<T> FindFirstAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dataSet.Where(predicate).FirstOrDefaultAsync();
    }

    #endregion Métodos públicos de Consulta




    #region Métodos públicos de Inclusão, Exclusão e Alteração


    /// <summary>
    /// Persiste a entidade na base de dados.
    /// </summary>
    /// <remarks>
    /// O savechanges automaticamente invalida no cache todas as referências à Entidade.
    /// </remarks>
    /// <param name="authenticatedUserName">Nome do usuário autenticado no identity, para ser registrado na entidade.</param>
    /// <param name="entity">Entidade a ser persistida.</param>
    /// <returns>bool</returns>
    /// 
    public virtual async Task AddAsync(string authenticatedUserName, T entity)
    {
        // Atualizo as datas e usuários de criação da entidade

        var data = DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("pt-BR"));
        var usuario = authenticatedUserName;

        Type entityType = entity.GetType();
        foreach (System.Reflection.PropertyInfo Info in entityType.GetProperties())
        {
            if (Info.Name == "DataCriacao" || Info.Name == "DataUltimaAlteracao") Info.SetValue(entity, data);
            if (Info.Name == "UsuarioCriacao" || Info.Name == "UsuarioUltimaAlteracao") Info.SetValue(entity, usuario);
        }

        await _dataSet.AddAsync(entity);
    }



    /// <summary>
    /// Atualiza a entidade na base de dados.
    /// </summary>
    /// <param name="authenticatedUserName">Nome do usuário autenticado no identity, para ser registrado na entidade.</param>
    /// <param name="entity">Entidade a ser persistida.</param>
    /// 
    public virtual async Task UpdateAsync(string authenticatedUserName, T entity)
    {
        // Atualizo as datas e usuários de alteração da entidade

        var data = DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("pt-BR"));
        var usuario = authenticatedUserName;

        Type entityType = entity.GetType();
        foreach (System.Reflection.PropertyInfo Info in entityType.GetProperties())
        {
            if (Info.Name == "DataUltimaAlteracao") Info.SetValue(entity, data);
            if (Info.Name == "UsuarioUltimaAlteracao") Info.SetValue(entity, usuario);
        }

        _dataSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }



    /// <summary>
    /// Remove um objeto da coleção da entidade.
    /// </summary>
    /// <param name="entity">Objeto com o tipo da entidade do repositório.</param>
    /// <returns>bool</returns>
    public async Task DeleteAsync(T entity)
    {
        _dataSet.Remove(entity);
    }

    #endregion Métodos públicos de Inclusão, Exclusão e Alteração




    #region Métodos públicos de apoio

    /// <summary>
    /// Desacopla uma entidade da coleção de memória rastreada pelo Entity Framework.
    /// </summary>
    /// <remarks>
    /// Utilização:
    /// int idParaDesanexar = 5;
    /// await DetachLocalAsync<Produto>(p => p.Id == idParaDesanexar);
    /// </remarks>
    /// <typeparam name="TDetach"></typeparam>
    /// <param name="predicate"></param>
    /// <returns>void</returns>
    public virtual async Task DetachLocalAsync<TDetach>(Func<TDetach, bool> predicate) where TDetach : class
    {
        await Task.Run(() =>
        {
            var local = _context.Set<TDetach>().Local.Where(predicate).FirstOrDefault();
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }
        });
    }

    #endregion Métodos públicos de apoio
}
