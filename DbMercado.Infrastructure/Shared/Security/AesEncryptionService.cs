using System.Security.Cryptography;
using System.Text;
using DbMercado.Application.Shared.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DbMercado.Infrastructure.Shared.Security;

/// <summary>
/// AES-256-GCM: payload persistido como Base64(nonce 12 bytes + tag 16 bytes + ciphertext).
/// Chave: variável de ambiente ou configuração <c>CHAT_ENCRYPTION_KEY</c> (Base64 de exatamente 32 bytes).
/// </summary>
public sealed class AesEncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;

    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var keyB64 = configuration["CHAT_ENCRYPTION_KEY"];
        if (string.IsNullOrWhiteSpace(keyB64))
        {
            throw new InvalidOperationException(
                "CHAT_ENCRYPTION_KEY não configurada. Defina a variável de ambiente ou entrada de configuração com a chave AES-256 em Base64 (32 bytes decodificados).");
        }

        try
        {
            _key = Convert.FromBase64String(keyB64.Trim());
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "CHAT_ENCRYPTION_KEY deve ser uma string Base64 válida.", ex);
        }

        if (_key.Length != KeySize)
        {
            throw new InvalidOperationException(
                $"CHAT_ENCRYPTION_KEY deve decodificar exatamente {KeySize} bytes (AES-256); obtido {_key.Length} byte(s).");
        }
    }

    /// <summary>Para design-time e testes: chave já materializada em 32 bytes.</summary>
    public AesEncryptionService(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != KeySize)
        {
            throw new InvalidOperationException(
                $"A chave AES-256 deve ter {KeySize} bytes; obtido {key.Length}.");
        }

        _key = (byte[])key.Clone();
    }

    public string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];
        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        var combined = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, combined, NonceSize + TagSize, cipherBytes.Length);
        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentNullException.ThrowIfNull(cipherText);
        if (cipherText.Length == 0)
        {
            return string.Empty;
        }

        byte[] combined;
        try
        {
            combined = Convert.FromBase64String(cipherText);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("Texto cifrado do chat não é Base64 válido.", ex);
        }

        if (combined.Length < NonceSize + TagSize)
        {
            throw new CryptographicException(
                "Payload cifrado do chat inválido (tamanho menor que nonce + tag).");
        }

        var nonce = combined.AsSpan(0, NonceSize);
        var tag = combined.AsSpan(NonceSize, TagSize);
        var cipher = combined.AsSpan(NonceSize + TagSize);
        var plainBytes = new byte[cipher.Length];
        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Decrypt(nonce, cipher, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }
}
