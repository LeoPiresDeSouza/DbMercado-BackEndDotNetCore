using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Administracao.Entities;

public class UsuarioEntity : BaseEntity
{
    public long Id { get; private set; }

    public long PessoaId { get; private set; }

    public string IdentityUserId { get; private set; }

    public bool Ativo { get; private set; }

    public PessoaEntity Pessoa { get; private set; } = null!;

    public UsuarioEntity()
    {
        IdentityUserId = string.Empty;
    }

    public static UsuarioEntity RegistrarNovoVinculo(
        long pessoaId,
        string identityUserId,
        string usuarioAuditoria)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new BusinessException("IDENTITY_OBRIGATORIO", "Identificador do usuário Identity é obrigatório.");
        if (pessoaId <= 0)
            throw new BusinessException("PESSOA_INVALIDA", "Pessoa inválida para vínculo.");

        var agora = DateTime.UtcNow;
        return new UsuarioEntity
        {
            PessoaId = pessoaId,
            IdentityUserId = identityUserId.Trim(),
            Ativo = true,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }

    public void Ativar(string usuarioAuditoria)
    {
        if (Ativo)
            return;
        Ativo = true;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    public void Inativar(string usuarioAuditoria)
    {
        if (!Ativo)
            return;
        Ativo = false;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    private void RegistrarAuditoriaAlteracao(string usuarioAuditoria)
    {
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }
}
