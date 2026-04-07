using System.Security.Cryptography;
using System.Text;

namespace DbMercado.Application.Chat;

/// <summary>Chave de cache alinhada a <c>CHAT_MODULE.md</c> (SHA256 do conteúdo + idioma alvo).</summary>
public static class TranslationTextHash
{
    public static string Compute(string content, string targetLang)
    {
        var bytes = Encoding.UTF8.GetBytes(content + targetLang);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
