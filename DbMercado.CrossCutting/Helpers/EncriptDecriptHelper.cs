using System.Security.Cryptography;

namespace DbMercado.CrossCutting.Helpers;


public static class EncriptDecriptHelper
{
    public static string EncryptAes(string input)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {

            aes.Key = Convert.FromBase64String(GetAesKey());
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                    {
                        streamWriter.Write(input);
                    }

                    array = memoryStream.ToArray();
                }
            }
        }

        return Convert.ToBase64String(array);
    }



    public static string DecryptAes(string input)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(input);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Convert.FromBase64String(GetAesKey());
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }



    private static string GetAesKey()
    {
        return "kbF+8CJZJ9Ssjp93487tt@2blkqartgbjnS*oj(_+*()JKJGsdf3RpQi/bAt1N31ms9d(*y%~ÇsR89hHbwP´~19HXYaBdjvBBEbCmc=";
    }
}
