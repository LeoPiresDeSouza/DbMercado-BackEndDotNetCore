namespace DbMercado.Application.Chat.Interfaces;

/// <summary>
/// Limite de envio de mensagens no hub (janela deslizante de 60s), alinhado a <c>CHAT_MODULE.md</c>.
/// </summary>
public interface IChatMensagemEnvioRateLimiter
{
    /// <summary>
    /// Registra um envio para o usuário. Retorna <c>false</c> se já houve 30 envios nos últimos 60s (UTC).
    /// </summary>
    /// <param name="retryAfterSeconds">Estimativa mínima de espera até caber na janela (≥ 1 quando retorna <c>false</c>).</param>
    bool TryAcquire(string identityUserId, out int retryAfterSeconds);
}
