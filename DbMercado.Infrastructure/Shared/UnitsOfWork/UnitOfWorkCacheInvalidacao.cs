using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Importacao.Entities;
using DbMercado.Domain.Produto.Entities;
using DbMercado.Infrastructure.Shared.Interfaces;

namespace DbMercado.Infrastructure.Shared.UnitsOfWork;

/// <summary>
/// Consolida a invalidação de cache por bounded context após <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync"/>.
/// Cada unit of work deve chamar <see cref="AposSaveSeAlterou"/> com o conjunto de entidades que seus repositórios podem persistir.
/// </summary>
public static class UnitOfWorkCacheInvalidacao
{
    public static void AposSaveSeAlterou(
        IApplicationCachingFactory factory,
        int linhasAfetadas,
        Action<IApplicationCachingFactory> invalidarCachesDaUnit)
    {
        if (linhasAfetadas > 0)
            invalidarCachesDaUnit(factory);
    }

    /// <summary>Entidades da UoW Administração: módulos, funcionalidades, permissões, refresh tokens.</summary>
    public static void Administracao(IApplicationCachingFactory f)
    {
        f.GetApplicationCaching<ModuloEntity>().InvalidateEntity();
        f.GetApplicationCaching<FuncionalidadeEntity>().InvalidateEntity();
        f.GetApplicationCaching<PermissaoEntity>().InvalidateEntity();
        f.GetApplicationCaching<RefreshTokenEntity>().InvalidateEntity();
    }

    /// <summary>Entidades da UoW Produto: catálogo, SKUs, categorias, mídias.</summary>
    public static void Produto(IApplicationCachingFactory f)
    {
        f.GetApplicationCaching<ProdutoEntity>().InvalidateEntity();
        f.GetApplicationCaching<SkuEntity>().InvalidateEntity();
        f.GetApplicationCaching<CategoriaProdutoEntity>().InvalidateEntity();
        f.GetApplicationCaching<MidiaEntity>().InvalidateEntity();
    }

    /// <summary>Entidades da UoW Importação: notas, itens, produtos importados.</summary>
    public static void Importacao(IApplicationCachingFactory f)
    {
        f.GetApplicationCaching<NotaFiscalEntity>().InvalidateEntity();
        f.GetApplicationCaching<ItemNotaFiscalEntity>().InvalidateEntity();
        f.GetApplicationCaching<ProdutoImportadoEntity>().InvalidateEntity();
    }

    /// <summary>Entidades da UoW Chat: salas, membros, convites, mensagens, traduções e recibos.</summary>
    public static void Chat(IApplicationCachingFactory f)
    {
        f.GetApplicationCaching<ChatRoomEntity>().InvalidateEntity();
        f.GetApplicationCaching<ChatMemberEntity>().InvalidateEntity();
        f.GetApplicationCaching<ChatInviteEntity>().InvalidateEntity();
        f.GetApplicationCaching<MessageEntity>().InvalidateEntity();
        f.GetApplicationCaching<MessageTranslationEntity>().InvalidateEntity();
        f.GetApplicationCaching<MessageReceiptEntity>().InvalidateEntity();
    }
}
