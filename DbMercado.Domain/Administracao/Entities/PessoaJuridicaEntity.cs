using DbMercado.Domain.Shared.Exceptions;
using DocFiscal = DbMercado.Domain.Administracao.ValueObjects.Documento;

namespace DbMercado.Domain.Administracao.Entities;

public class PessoaJuridicaEntity : BaseEntity
{
    public long PessoaId { get; private set; }

    public string RazaoSocial { get; private set; }

    public string NomeFantasia { get; private set; }

    public string Cnpj { get; private set; }

    public PessoaEntity Pessoa { get; private set; } = null!;

    public PessoaJuridicaEntity()
    {
        RazaoSocial = string.Empty;
        NomeFantasia = string.Empty;
        Cnpj = string.Empty;
    }

    internal static PessoaJuridicaEntity CriarParaRaiz(
        PessoaEntity raiz,
        string razaoSocial,
        string? nomeFantasia,
        DocFiscal cnpj,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(raiz);
        ArgumentNullException.ThrowIfNull(cnpj);
        if (!cnpj.EhCnpj)
            throw new ArgumentException("Documento deve ser CNPJ.", nameof(cnpj));

        var agora = DateTime.UtcNow;
        var entidade = new PessoaJuridicaEntity
        {
            Pessoa = raiz,
            PessoaId = raiz.Id,
            RazaoSocial = razaoSocial?.Trim() ?? string.Empty,
            NomeFantasia = nomeFantasia?.Trim() ?? string.Empty,
            Cnpj = cnpj.ValorNormalizado,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };

        if (string.IsNullOrWhiteSpace(entidade.RazaoSocial))
            throw new BusinessException("RAZAO_SOCIAL_OBRIGATORIA", "Razão social é obrigatória.");

        return entidade;
    }

    public void AtualizarDadosEmpresariais(string razaoSocial, string? nomeFantasia, string usuarioAuditoria)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new BusinessException("RAZAO_SOCIAL_OBRIGATORIA", "Razão social é obrigatória.");

        RazaoSocial = razaoSocial.Trim();
        NomeFantasia = nomeFantasia?.Trim() ?? string.Empty;
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    public DocFiscal ObterCnpj()
    {
        if (string.IsNullOrWhiteSpace(Cnpj))
            throw new InvalidOperationException("CNPJ não definido.");
        return DocFiscal.CriarCnpj(Cnpj);
    }
}
