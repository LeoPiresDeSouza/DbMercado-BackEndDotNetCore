using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Administracao.Entities;

public class PessoaEmailEntity : BaseEntity
{
    public long Id { get; private set; }

    public long PessoaId { get; private set; }

    public string Email { get; private set; }

    public string TipoEmail { get; private set; }

    public PessoaEntity Pessoa { get; private set; } = null!;

    public PessoaEmailEntity()
    {
        Email = string.Empty;
        TipoEmail = string.Empty;
    }

    internal static PessoaEmailEntity CriarNovaEntrada(
        PessoaEntity pessoa,
        string email,
        string tipoEmail,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(pessoa);
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessException("EMAIL_OBRIGATORIO", "E-mail é obrigatório.");

        var agora = DateTime.UtcNow;
        return new PessoaEmailEntity
        {
            Pessoa = pessoa,
            PessoaId = pessoa.Id,
            Email = email.Trim(),
            TipoEmail = tipoEmail?.Trim() ?? string.Empty,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }
}
