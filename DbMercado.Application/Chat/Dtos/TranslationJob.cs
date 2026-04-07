namespace DbMercado.Application.Chat.Dtos;

/// <summary>Job interno enfileirado para o worker de tradução (Etapa 4).</summary>
public sealed record TranslationJob(
    Guid MessageId,
    string Content,
    string SourceLang,
    IReadOnlyCollection<string> TargetLangs,
    Guid RoomId);
