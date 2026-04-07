namespace DbMercado.Application.Shared.Interfaces;

/// <summary>
/// Criptografia simétrica para campos sensíveis persistidos via EF (ex.: conteúdo de mensagens de chat).
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
