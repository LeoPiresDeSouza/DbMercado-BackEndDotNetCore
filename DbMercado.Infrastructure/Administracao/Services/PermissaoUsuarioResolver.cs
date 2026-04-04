using System.Security.Claims;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Infrastructure.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Administracao.Services;

public sealed class PermissaoUsuarioResolver : IPermissaoUsuarioResolver
{
    private const string ClaimPermissao = "Permissao";

    private readonly AppDbContext _context;

    public PermissaoUsuarioResolver(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UsuarioPossuiPermissaoAsync(
        ClaimsPrincipal usuario,
        string funcionalidadeNomeNormalizado,
        string permissaoNome,
        CancellationToken cancellationToken = default)
    {
        if (usuario.Identity?.IsAuthenticated != true)
            return false;

        var permissaoId = await _context.Permissoes.AsNoTracking()
            .Where(p => p.Permissao == permissaoNome
                        && p.Funcionalidade.NomeNormalizado == funcionalidadeNomeNormalizado)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (permissaoId == 0)
            return false;

        var idStr = permissaoId.ToString();
        return usuario.Claims.Any(c => c.Type == ClaimPermissao && c.Value == idStr);
    }
}
