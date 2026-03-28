using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Administracao.Entities;

public class PessoaTelefoneEntity : BaseEntity
{
    public long Id { get; private set; }

    public long PessoaId { get; private set; }

    public string TipoTelefone { get; private set; }

    public string Ddd { get; private set; }

    public string Numero { get; private set; }

    public PessoaEntity Pessoa { get; private set; } = null!;

    public PessoaTelefoneEntity()
    {
        TipoTelefone = string.Empty;
        Ddd = string.Empty;
        Numero = string.Empty;
    }

    internal static PessoaTelefoneEntity CriarNovaEntrada(
        PessoaEntity pessoa,
        string tipoTelefone,
        string ddd,
        string numero,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(pessoa);
        if (string.IsNullOrWhiteSpace(tipoTelefone))
            throw new BusinessException("TIPO_TELEFONE_OBRIGATORIO", "Tipo de telefone é obrigatório.");
        if (string.IsNullOrWhiteSpace(ddd))
            throw new BusinessException("DDD_OBRIGATORIO", "DDD é obrigatório.");
        if (string.IsNullOrWhiteSpace(numero))
            throw new BusinessException("NUMERO_TELEFONE_OBRIGATORIO", "Número do telefone é obrigatório.");

        var agora = DateTime.UtcNow;
        return new PessoaTelefoneEntity
        {
            Pessoa = pessoa,
            PessoaId = pessoa.Id,
            TipoTelefone = tipoTelefone.Trim(),
            Ddd = ddd.Trim(),
            Numero = numero.Trim(),
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }
}
