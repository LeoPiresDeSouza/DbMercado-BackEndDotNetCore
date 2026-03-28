using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DeepBlues.Infrastructure.Providers;

/// <summary>
/// Responsável por gerenciar instâncias de <see cref="CancellationTokenSource"/> utilizadas
/// para invalidação de cache por chave lógica (normalmente associada a uma entidade).
///
/// A implementação utiliza <see cref="ConcurrentDictionary{TKey, TValue}"/> combinado com
/// <see cref="Lazy{T}"/> para garantir:
/// - Criação thread-safe
/// - Instância única por chave
/// - Evitar criação duplicada em cenários concorrentes
///
/// Cada chave representa um agrupamento lógico de entradas de cache que podem ser invalidadas em conjunto.
/// </summary>
public static class ApplicationCachingCancellationTokensFactory
{
    /// <summary>
    /// Estrutura thread-safe que armazena os tokens por chave lógica.
    /// O uso de Lazy garante que o token seja criado apenas quando necessário.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Lazy<CancellationTokenSource>> _tokens = new();

    /// <summary>
    /// Retorna a quantidade de tokens atualmente gerenciados.
    /// </summary>
    public static int Count => _tokens.Count;

    /// <summary>
    /// Obtém um <see cref="CancellationTokenSource"/> associado à chave informada.
    /// Caso não exista, ele será criado de forma thread-safe.
    /// </summary>
    /// <param name="key">Chave lógica do token (ex: nome completo da entidade).</param>
    /// <returns>Instância única de <see cref="CancellationTokenSource"/>.</returns>
    /// <exception cref="ArgumentException">Lançada quando a chave é inválida.</exception>
    public static CancellationTokenSource Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache token key cannot be null or empty.", nameof(key));

        var lazy = _tokens.GetOrAdd(key, _ =>
            new Lazy<CancellationTokenSource>(
                () => new CancellationTokenSource(),
                LazyThreadSafetyMode.ExecutionAndPublication));

        return lazy.Value;
    }

    /// <summary>
    /// Cancela e remove o token associado à chave.
    /// Essa operação invalida automaticamente todas as entradas de cache
    /// vinculadas a esse token.
    /// </summary>
    /// <param name="key">Chave lógica do token.</param>
    public static void Reset(string key)
    {
        if (_tokens.TryRemove(key, out var lazy) && lazy.IsValueCreated)
        {
            var token = lazy.Value;

            if (!token.IsCancellationRequested)
                token.Cancel();

            token.Dispose();
        }
    }

    /// <summary>
    /// Cancela e remove todos os tokens gerenciados.
    /// Deve ser utilizado com cautela, pois invalida todo o cache associado.
    /// </summary>
    public static void Clear()
    {
        foreach (var item in _tokens.Values)
        {
            if (item.IsValueCreated)
            {
                var token = item.Value;

                if (!token.IsCancellationRequested)
                    token.Cancel();

                token.Dispose();
            }
        }

        _tokens.Clear();
    }
}
