using DbMercado.Domain.Shared.Exceptions;
using DocFiscal = DbMercado.Domain.Administracao.ValueObjects.Documento;

namespace DbMercado.Domain.Administracao.Entities;

public class PessoaFisicaEntity : BaseEntity
{
    public long PessoaId { get; private set; }

    public string Nome { get; private set; }

    public string Sobrenome { get; private set; }

    public string Documento { get; private set; }

    public string TipoDocumento { get; private set; }

    public string Sexo { get; private set; }

    public DateTime? DataNascimento { get; private set; }

    public PessoaEntity Pessoa { get; private set; } = null!;

    public PessoaFisicaEntity()
    {
        Nome = string.Empty;
        Sobrenome = string.Empty;
        Documento = string.Empty;
        TipoDocumento = string.Empty;
        Sexo = string.Empty;
    }

    /// <summary>
    /// Cria o registro 1:1 da pessoa física já vinculado ao agregado (uso na composição da raiz).
    /// </summary>
    internal static PessoaFisicaEntity CriarParaRaiz(
        PessoaEntity raiz,
        string nome,
        string? sobrenome,
        DocFiscal cpf,
        string sexo,
        DateTime? dataNascimento,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(raiz);
        ArgumentNullException.ThrowIfNull(cpf);
        if (!cpf.EhCpf)
            throw new ArgumentException("Documento deve ser CPF.", nameof(cpf));

        var agora = DateTime.UtcNow;
        var entidade = new PessoaFisicaEntity
        {
            Pessoa = raiz,
            PessoaId = raiz.Id,
            Nome = nome?.Trim() ?? string.Empty,
            Sobrenome = sobrenome?.Trim() ?? string.Empty,
            Documento = cpf.ValorNormalizado,
            TipoDocumento = "CPF",
            Sexo = sexo?.Trim() ?? string.Empty,
            DataNascimento = dataNascimento,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };

        if (string.IsNullOrWhiteSpace(entidade.Nome))
            throw new BusinessException("NOME_OBRIGATORIO", "Nome é obrigatório para pessoa física.");

        return entidade;
    }

    /// <summary>
    /// Cria pessoa física com documento não fiscal (ex.: passaporte), imutável após criação.
    /// </summary>
    internal static PessoaFisicaEntity CriarParaRaizComDocumentoGenerico(
        PessoaEntity raiz,
        string nome,
        string? sobrenome,
        string tipoDocumento,
        string numeroDocumento,
        string sexo,
        DateTime? dataNascimento,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(raiz);
        if (string.Equals(tipoDocumento?.Trim(), "CPF", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("CPF_REQUER_VO", "Para CPF utilize o factory com o value object Documento.");

        var agora = DateTime.UtcNow;
        var entidade = new PessoaFisicaEntity
        {
            Pessoa = raiz,
            PessoaId = raiz.Id,
            Nome = nome?.Trim() ?? string.Empty,
            Sobrenome = sobrenome?.Trim() ?? string.Empty,
            Documento = numeroDocumento?.Trim() ?? string.Empty,
            TipoDocumento = tipoDocumento?.Trim() ?? string.Empty,
            Sexo = sexo?.Trim() ?? string.Empty,
            DataNascimento = dataNascimento,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };

        if (string.IsNullOrWhiteSpace(entidade.Nome))
            throw new BusinessException("NOME_OBRIGATORIO", "Nome é obrigatório para pessoa física.");
        if (string.IsNullOrWhiteSpace(entidade.Documento))
            throw new BusinessException("DOCUMENTO_OBRIGATORIO", "Número do documento é obrigatório.");

        return entidade;
    }

    public void AtualizarDadosPessoais(
        string nome,
        string? sobrenome,
        string? sexo,
        DateTime? dataNascimento,
        string usuarioAuditoria)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessException("NOME_OBRIGATORIO", "Nome é obrigatório.");

        Nome = nome.Trim();
        Sobrenome = sobrenome?.Trim() ?? string.Empty;
        Sexo = sexo?.Trim() ?? string.Empty;
        DataNascimento = dataNascimento;
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    /// <summary>
    /// Reconstrói o documento fiscal quando o cadastro é CPF (somente leitura de domínio).
    /// </summary>
    public DocFiscal? ObterDocumentoCpf()
    {
        if (!string.Equals(TipoDocumento, "CPF", StringComparison.OrdinalIgnoreCase))
            return null;
        try
        {
            return DocFiscal.CriarCpf(Documento);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
