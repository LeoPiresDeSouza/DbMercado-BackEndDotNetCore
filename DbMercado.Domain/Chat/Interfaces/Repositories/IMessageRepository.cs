using DbMercado.Domain.Chat.Entities;

namespace DbMercado.Domain.Chat.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<MessageEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MessageEntity?> ObterPorIdComTraducoesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mensagens visíveis da sala, ordenadas por <see cref="MessageEntity.SentAt"/> descendente (mais recentes primeiro).
    /// </summary>
    Task<(IReadOnlyList<MessageEntity> Itens, int Total)> ListarPorSalaPaginadoAsync(
        Guid roomId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(string usuarioAutenticado, MessageEntity entity, CancellationToken cancellationToken = default);

    Task AtualizarAsync(string usuarioAutenticado, MessageEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria ou atualiza recibo de leitura para o par (mensagem, usuário). <see cref="MessageReceiptEntity.DeliveredAt"/> permanece nulo neste fluxo.
    /// </summary>
    Task RegistrarOuAtualizarLeituraAsync(
        string usuarioAutenticado,
        Guid messageId,
        string userId,
        DateTime readAtUtc,
        CancellationToken cancellationToken = default);

    /// <summary>Mensagens com tradução ainda pendente (não removidas).</summary>
    Task<IReadOnlyList<MessageEntity>> ListarMensagensTraducaoPendenteAsync(
        CancellationToken cancellationToken = default);

    Task<MessageEntity?> ObterPorIdComTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> TraducaoExisteAsync(
        Guid messageId,
        string targetLang,
        CancellationToken cancellationToken = default);

    Task AdicionarTraducaoAsync(
        string usuarioAutenticado,
        MessageTranslationEntity entity,
        CancellationToken cancellationToken = default);
}
