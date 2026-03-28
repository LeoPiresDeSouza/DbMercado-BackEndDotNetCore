using DbMercado.Application.Importacao.Dtos;
using DbMercado.Application.Importacao.Interfaces;
using DbMercado.Domain.Importacao.Entities;
using DbMercado.Domain.Importacao.Interfaces.UnitsOfWork;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Application.Importacao.Services;

public class ImportacaoService : IImportacaoService
{
    private readonly IUwImportacao _uw;

    public ImportacaoService(IUwImportacao uw)
    {
        _uw = uw;
    }

    public async Task<long> CadastrarNotaFiscalAsync(
        string usuarioAutenticado,
        NotaFiscalCadastroRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _uw.NotaFiscalRepository.GetByChaveAcessoAsync(request.ChaveAcesso, cancellationToken) is not null)
            throw new BusinessException("NF_CHAVE_DUPLICADA", "Já existe nota fiscal com esta chave de acesso.");

        var nota = new NotaFiscalEntity
        {
            ChaveAcesso = request.ChaveAcesso.Trim(),
            Numero = request.Numero.Trim(),
            Serie = request.Serie.Trim(),
            DataEmissao = request.DataEmissao,
            CnpjEmitente = request.CnpjEmitente.Trim(),
            RazaoSocialEmitente = request.RazaoSocialEmitente?.Trim(),
            ValorTotal = request.ValorTotal,
            Itens = request.Itens.Select(i => new ItemNotaFiscalEntity
            {
                NumeroItem = i.NumeroItem,
                CodigoProdutoFornecedor = i.CodigoProdutoFornecedor?.Trim(),
                Descricao = i.Descricao.Trim(),
                Ncm = i.Ncm?.Trim(),
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorTotalLinha = i.ValorTotalLinha
            }).ToList()
        };

        await _uw.NotaFiscalRepository.AddComItensAsync(usuarioAutenticado, nota, cancellationToken);
        await _uw.SaveChangesAsync(cancellationToken);
        return nota.Id;
    }

    public async Task<NotaFiscalResponse?> ObterNotaFiscalAsync(long id, CancellationToken cancellationToken = default)
    {
        var nf = await _uw.NotaFiscalRepository.GetByIdWithItensAsync(id, cancellationToken);
        return nf is null ? null : MapearNota(nf);
    }

    public async Task<IReadOnlyList<NotaFiscalResumoResponse>> ListarNotasFiscaisAsync(CancellationToken cancellationToken = default)
    {
        var lista = await _uw.NotaFiscalRepository.FindCollectionAsync(_ => true);
        return lista
            .OrderByDescending(n => n.DataEmissao)
            .Select(n => new NotaFiscalResumoResponse
            {
                Id = n.Id,
                ChaveAcesso = n.ChaveAcesso,
                Numero = n.Numero,
                DataEmissao = n.DataEmissao,
                ValorTotal = n.ValorTotal
            })
            .ToList();
    }

    public async Task<long> CadastrarProdutoImportadoAsync(
        string usuarioAutenticado,
        ProdutoImportadoCadastroRequest request,
        CancellationToken cancellationToken = default)
    {
        var codigo = request.CodigoInterno.Trim();
        if (await _uw.ProdutoImportadoRepository.GetByCodigoInternoAsync(codigo, cancellationToken) is not null)
            throw new BusinessException("PRODUTO_CODIGO_DUPLICADO", "Já existe produto importado com este código interno.");

        if (request.ItemNotaFiscalOrigemId is long itemId)
        {
            if (!await _uw.NotaFiscalRepository.ItemNotaFiscalExistsAsync(itemId, cancellationToken))
                throw new BusinessException("NF_ITEM_INEXISTENTE", "Item de nota fiscal de origem não encontrado.");
        }

        var entity = new ProdutoImportadoEntity
        {
            CodigoInterno = codigo,
            Descricao = request.Descricao.Trim(),
            Ncm = request.Ncm?.Trim(),
            UnidadeMedida = request.UnidadeMedida?.Trim(),
            ItemNotaFiscalOrigemId = request.ItemNotaFiscalOrigemId
        };

        await _uw.ProdutoImportadoRepository.AddAsync(usuarioAutenticado, entity);
        await _uw.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<ProdutoImportadoResponse?> ObterProdutoImportadoAsync(long id, CancellationToken cancellationToken = default)
    {
        var p = await _uw.ProdutoImportadoRepository.GetByIdComOrigemAsync(id, cancellationToken);
        if (p is null)
            return null;

        return new ProdutoImportadoResponse
        {
            Id = p.Id,
            CodigoInterno = p.CodigoInterno,
            Descricao = p.Descricao,
            Ncm = p.Ncm,
            UnidadeMedida = p.UnidadeMedida,
            ItemNotaFiscalOrigemId = p.ItemNotaFiscalOrigemId,
            NotaFiscalOrigemId = p.ItemNotaFiscalOrigem?.NotaFiscalId,
            NotaFiscalChaveAcesso = p.ItemNotaFiscalOrigem?.NotaFiscal?.ChaveAcesso
        };
    }

    public async Task<IReadOnlyList<ProdutoImportadoResumoResponse>> ListarProdutosImportadosAsync(CancellationToken cancellationToken = default)
    {
        var lista = await _uw.ProdutoImportadoRepository.FindCollectionAsync(_ => true);
        return lista
            .OrderBy(p => p.CodigoInterno)
            .Select(p => new ProdutoImportadoResumoResponse
            {
                Id = p.Id,
                CodigoInterno = p.CodigoInterno,
                Descricao = p.Descricao
            })
            .ToList();
    }

    private static NotaFiscalResponse MapearNota(NotaFiscalEntity nf)
    {
        return new NotaFiscalResponse
        {
            Id = nf.Id,
            ChaveAcesso = nf.ChaveAcesso,
            Numero = nf.Numero,
            Serie = nf.Serie,
            DataEmissao = nf.DataEmissao,
            CnpjEmitente = nf.CnpjEmitente,
            RazaoSocialEmitente = nf.RazaoSocialEmitente,
            ValorTotal = nf.ValorTotal,
            Itens = nf.Itens
                .OrderBy(i => i.NumeroItem)
                .Select(i => new ItemNotaFiscalResponse
                {
                    Id = i.Id,
                    NumeroItem = i.NumeroItem,
                    CodigoProdutoFornecedor = i.CodigoProdutoFornecedor,
                    Descricao = i.Descricao,
                    Ncm = i.Ncm,
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario,
                    ValorTotalLinha = i.ValorTotalLinha
                })
                .ToList()
        };
    }
}
