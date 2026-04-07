namespace DbMercado.Application.Chat;

/// <summary>Política de limite de envio no <c>ChatHub</c> (CHAT_MODULE / etapa 7).</summary>
public static class ChatMensagemEnvioRateLimitPolicy
{
    public const int MaxPorJanela = 30;

    public static readonly TimeSpan Janela = TimeSpan.FromSeconds(60);
}
