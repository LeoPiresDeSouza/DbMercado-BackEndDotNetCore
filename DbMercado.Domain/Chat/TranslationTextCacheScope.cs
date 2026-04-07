namespace DbMercado.Domain.Chat;

/// <summary>
/// Marcador de escopo para o cache de resultados de tradução (hash do texto + idioma alvo).
/// Não é entidade EF; isola o token de invalidação do cache de <see cref="Entities.MessageTranslationEntity"/>,
/// que é invalidado pelo Unit of Work do chat ao persistir mensagens/traduções.
/// </summary>
public sealed class TranslationTextCacheScope
{
}
