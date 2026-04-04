namespace DbMercado.Application.Administracao.Dtos.AppLog;

public class LogBackupArquivoDto
{
    public string Nome { get; set; } = string.Empty;

    public long TamanhoBytes { get; set; }

    public DateTimeOffset DataCriacao { get; set; }
}
