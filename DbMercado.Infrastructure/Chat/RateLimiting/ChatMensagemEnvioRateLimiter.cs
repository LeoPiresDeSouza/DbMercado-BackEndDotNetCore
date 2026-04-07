using System.Collections.Concurrent;
using DbMercado.Application.Chat;
using DbMercado.Application.Chat.Interfaces;

namespace DbMercado.Infrastructure.Chat.RateLimiting;

/// <summary>
/// Janela deslizante por <c>IdentityUser.Id</c>, conforme <see cref="ChatMensagemEnvioRateLimitPolicy"/>.
/// </summary>
public sealed class ChatMensagemEnvioRateLimiter : IChatMensagemEnvioRateLimiter
{

    private sealed class EstadoUsuario
    {
        public readonly Queue<DateTime> InstantUtc = new();
        public readonly object Portao = new();
    }

    private readonly ConcurrentDictionary<string, EstadoUsuario> _porUsuario = new();

    /// <inheritdoc />
    public bool TryAcquire(string identityUserId, out int retryAfterSeconds)
    {
        retryAfterSeconds = 0;
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            return true;
        }

        var estado = _porUsuario.GetOrAdd(identityUserId, static _ => new EstadoUsuario());
        var agora = DateTime.UtcNow;
        lock (estado.Portao)
        {
            while (
                estado.InstantUtc.Count > 0
                && (agora - estado.InstantUtc.Peek()) >= ChatMensagemEnvioRateLimitPolicy.Janela)
            {
                estado.InstantUtc.Dequeue();
            }

            if (estado.InstantUtc.Count >= ChatMensagemEnvioRateLimitPolicy.MaxPorJanela)
            {
                var maisAntigo = estado.InstantUtc.Peek();
                var segundosRestantes =
                    ChatMensagemEnvioRateLimitPolicy.Janela.TotalSeconds - (agora - maisAntigo).TotalSeconds;
                retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(segundosRestantes));
                return false;
            }

            estado.InstantUtc.Enqueue(agora);
            return true;
        }
    }
}
