using System.ComponentModel.DataAnnotations;

namespace DbMercado.Domain.Shared.Entities;

public abstract class BaseEntity
{
    /// <summary>
    /// Token de concorrência otimista (coluna <c>rowversion</c> no SQL Server quando mapeado no EF Core).
    /// Em entidades sem mapeamento de concorrência, permanece nulo.
    /// </summary>
    public byte[]? RowVersion { get; set; }

    [Display(Name = "Data de criação")]
    public DateTime DataCriacao { get; set; }

    [Display(Name = "Data da última alteração")]
    public DateTime DataUltimaAlteracao { get; set; }

    [Display(Name = "Usuário de criação")]
    public string UsuarioCriacao { get; set; } = string.Empty;

    [Display(Name = "Usuário da última alteração")]
    public string UsuarioUltimaAlteracao { get; set; } = string.Empty;
}
