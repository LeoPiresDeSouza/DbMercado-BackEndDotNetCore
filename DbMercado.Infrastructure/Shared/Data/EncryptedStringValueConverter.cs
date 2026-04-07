using DbMercado.Application.Shared.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DbMercado.Infrastructure.Shared.Data;

/// <summary>
/// Converte entre texto de domínio (claro) e coluna (cifrado em Base64) usando <see cref="IEncryptionService"/>.
/// </summary>
public sealed class EncryptedStringValueConverter : ValueConverter<string, string>
{
    public EncryptedStringValueConverter(IEncryptionService encryption)
        : base(
            plain => encryption.Encrypt(plain),
            cipher => encryption.Decrypt(cipher))
    {
    }
}
