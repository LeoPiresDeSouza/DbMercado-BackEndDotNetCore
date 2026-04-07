using DbMercado.Application.Chat.Dtos;

namespace DbMercado.Application.Chat.HubClients;

/// <summary>Contrato dos métodos invocados no cliente SignalR do chat.</summary>
public interface IChatHubClient
{
    Task ReceiveMessage(MessageDto message);

    Task ReceiveTranslation(Guid messageId, string translatedText, string targetLang);

    /// <summary>
    /// Traduzir para o idioma do membro falhou; o cliente deve exibir o texto original e tratar o estado de falha.
    /// Enviado apenas a usuários cuja preferência de idioma na sala difere do idioma de origem da mensagem.
    /// </summary>
    Task TranslationFailed(Guid messageId);

    /// <param name="userId">ID do usuário no ASP.NET Identity (<c>AspNetUsers.Id</c>).</param>
    Task MessageRead(Guid messageId, string userId);

    /// <param name="userId">ID do usuário no ASP.NET Identity (<c>AspNetUsers.Id</c>).</param>
    Task UserTyping(Guid roomId, string userId, bool isTyping);

    Task UserInvited(InviteResponse invite);
}
