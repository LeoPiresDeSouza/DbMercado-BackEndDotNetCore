using DbMercado.Domain.Shared.Exceptions;
using DocFiscal = DbMercado.Domain.Administracao.ValueObjects.Documento;
using TipoPessoaCadastro = DbMercado.Domain.Administracao.Enums.TipoPessoa;

namespace DbMercado.Domain.Administracao.Entities;

/// <summary>
/// Raiz de agregado de cadastro de pessoa (física ou jurídica).
/// </summary>
public class PessoaEntity : BaseEntity
{
    public long Id { get; private set; }

    public TipoPessoaCadastro TipoPessoa { get; private set; }

    /// <summary>Indica se o cadastro está ativo para operações de negócio.</summary>
    public bool Ativa { get; private set; }

    public ICollection<PessoaTelefoneEntity> Telefones { get; private set; }

    public ICollection<PessoaEmailEntity> Emails { get; private set; }

    public ICollection<PessoaEnderecoEntity> Enderecos { get; private set; }

    public ICollection<PessoaRedeSocialEntity> RedesSociais { get; private set; }

    public PessoaFisicaEntity? Fisica { get; private set; }

    public PessoaJuridicaEntity? Juridica { get; private set; }

    public PessoaEntity()
    {
        TipoPessoa = TipoPessoaCadastro.Indefinido;
        Ativa = true;
        Telefones = new List<PessoaTelefoneEntity>();
        Emails = new List<PessoaEmailEntity>();
        Enderecos = new List<PessoaEnderecoEntity>();
        RedesSociais = new List<PessoaRedeSocialEntity>();
    }

    public static PessoaEntity RegistrarNovaPessoaFisica(
        string nome,
        string? sobrenome,
        DocFiscal cpf,
        string sexo,
        DateTime? dataNascimento,
        string usuarioAuditoria)
    {
        var raiz = CriarRaiz(TipoPessoaCadastro.Fisica, usuarioAuditoria);
        raiz.Fisica = PessoaFisicaEntity.CriarParaRaiz(raiz, nome, sobrenome, cpf, sexo, dataNascimento, usuarioAuditoria);
        return raiz;
    }

    public static PessoaEntity RegistrarNovaPessoaFisicaComDocumentoGenerico(
        string nome,
        string? sobrenome,
        string tipoDocumento,
        string numeroDocumento,
        string sexo,
        DateTime? dataNascimento,
        string usuarioAuditoria)
    {
        var raiz = CriarRaiz(TipoPessoaCadastro.Fisica, usuarioAuditoria);
        raiz.Fisica = PessoaFisicaEntity.CriarParaRaizComDocumentoGenerico(
            raiz, nome, sobrenome, tipoDocumento, numeroDocumento, sexo, dataNascimento, usuarioAuditoria);
        return raiz;
    }

    public static PessoaEntity RegistrarNovaPessoaJuridica(
        string razaoSocial,
        string? nomeFantasia,
        DocFiscal cnpj,
        string usuarioAuditoria)
    {
        var raiz = CriarRaiz(TipoPessoaCadastro.Juridica, usuarioAuditoria);
        raiz.Juridica = PessoaJuridicaEntity.CriarParaRaiz(raiz, razaoSocial, nomeFantasia, cnpj, usuarioAuditoria);
        return raiz;
    }

    public void AtualizarDadosBasicos(
        string nome,
        string? sobrenome,
        string? sexo,
        DateTime? dataNascimento,
        string usuarioAuditoria)
    {
        GarantirAgregadoAtivo();
        if (TipoPessoa != TipoPessoaCadastro.Fisica || Fisica is null)
            throw new BusinessException("PESSOA_NAO_FISICA", "Só é possível atualizar estes dados para pessoa física com detalhe cadastrado.");

        Fisica.AtualizarDadosPessoais(nome, sobrenome, sexo, dataNascimento, usuarioAuditoria);
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    public void AtualizarDadosBasicos(string razaoSocial, string? nomeFantasia, string usuarioAuditoria)
    {
        GarantirAgregadoAtivo();
        if (TipoPessoa != TipoPessoaCadastro.Juridica || Juridica is null)
            throw new BusinessException("PESSOA_NAO_JURIDICA", "Só é possível atualizar estes dados para pessoa jurídica com detalhe cadastrado.");

        Juridica.AtualizarDadosEmpresariais(razaoSocial, nomeFantasia, usuarioAuditoria);
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    public void AdicionarTelefone(string tipoTelefone, string ddd, string numero, string usuarioAuditoria)
    {
        GarantirAgregadoAtivo();
        var entidade = PessoaTelefoneEntity.CriarNovaEntrada(this, tipoTelefone, ddd, numero, usuarioAuditoria);
        Telefones.Add(entidade);
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    public void AdicionarEmail(string email, string tipoEmail, string usuarioAuditoria)
    {
        GarantirAgregadoAtivo();
        var entidade = PessoaEmailEntity.CriarNovaEntrada(this, email, tipoEmail, usuarioAuditoria);
        Emails.Add(entidade);
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    public void Ativar(string usuarioAuditoria)
    {
        if (Ativa)
            return;
        Ativa = true;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    public void Inativar(string usuarioAuditoria)
    {
        if (!Ativa)
            return;
        Ativa = false;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    private static PessoaEntity CriarRaiz(TipoPessoaCadastro tipo, string usuarioAuditoria)
    {
        var agora = DateTime.UtcNow;
        return new PessoaEntity
        {
            TipoPessoa = tipo,
            Ativa = true,
            Telefones = new List<PessoaTelefoneEntity>(),
            Emails = new List<PessoaEmailEntity>(),
            Enderecos = new List<PessoaEnderecoEntity>(),
            RedesSociais = new List<PessoaRedeSocialEntity>(),
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }

    private void GarantirAgregadoAtivo()
    {
        if (!Ativa)
            throw new BusinessException("PESSOA_INATIVA", "Operação não permitida para pessoa inativa.");
    }

    private void RegistrarAuditoriaAlteracao(string usuarioAuditoria)
    {
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }
}
