namespace DbMercado.CrossCutting.Helpers;

public static class CreatePasswordHelper
{
    private static string[] caracteres = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" , "@", "#", "$", "%", "&", "!", "(", ")", "{", "}", "|", "=", "+", "-", "_" };

    public static string GerarSenha(int? length = 8)
    {
        ArgumentNullException.ThrowIfNull(length, nameof(length));
        if (length < 8 || length > 20) throw new ArgumentOutOfRangeException(nameof(length), "O tamanho da senha deve estar entre 8 e 20 caracteres.");

        Random rand = new Random();

        List<string> tempPasswd = new List<string>();
        List<int> numGerados = new();
        List<int> numGeradosIniciais = new();
        int maxTries = 10;
        int index = 0;

        // Primeiro garanto que pelo menos ums dos valores previstos estará na senha solicitada.
               
        for (int i = 0; i < 5; i++)
        {
            index = rand.Next(1, 5);
            while (numGeradosIniciais.Contains(index)){ index = rand.Next(1, 5);}
            numGeradosIniciais.Add(index);

            switch (index)
            {
                case 1:
                    getLower(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 2:
                    getSpecial(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 3:
                    getAny(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 4:
                    getUpper(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 5:
                    getAny(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
            }
        }

        // Preencho o restante da senha com caracteres aleatórios, respeitando o tamanho solicitado.

        index = 0;
        for (int i = 0; i < length -5; i++)
        {
            index = rand.Next(1, 50);

            switch (index)
            {
                case 1:case 6:case 11:case 16:case 21:case 26:case 31:case 36: case 41:case 46:
                    getAny(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 2:case 7:case 12:case 17:case 22:case 27:case 32:case 37:case 42:case 47:
                    getUpper(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 3:case 8:case 13:case 18:case 23:case 28:case 33:case 38:case 43:case 48:
                    getLower(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 4:case 9:case 14:case 19:case 24:case 29:case 34:case 39:case 44:case 49:
                    getAny(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
                case 5:case 10:case 15:case 20:case 25:case 30:case 35:case 40:case 45:case 50:
                    getSpecial(rand, maxTries, ref numGerados, ref tempPasswd);
                    break;
            }
        }

        // Embaralho a senha gerada

        int first = 0;
        int second = 0;
        string passwdFirst = string.Empty;
        string passwdSecond = string.Empty;

        for (int i = 0; i < 10; i++)
        {
            first = rand.Next(0, 9);
            second = rand.Next(0, 9);

            while (second == first) { second = rand.Next(0, 9); }

            passwdFirst = tempPasswd[first];
            passwdSecond = tempPasswd[second];

            tempPasswd[first] = passwdSecond;
            tempPasswd[second] = passwdFirst;
        }

        // retorno a senha gerada
        string finalPasswd = string.Empty;
        foreach (string passwd in tempPasswd) { finalPasswd += passwd; }

        return finalPasswd;
    }



    public static string GerarSenhaNumerica(int length = 6)
    {
        // Calculo o valor mínimo e máximo em função do tamanho da senha solicitado.

        int beginValue = Convert.ToInt32("1".PadRight(length, '0'));    
        int endValue = Convert.ToInt32("9".PadRight(length, '9'));

        Random rand = new Random();
        return rand.Next(beginValue, endValue).ToString();
    }


    #region Métodos privados de apoio

    private static void getUpper(Random rand, int maxTries, ref List<int> numGerados, ref List<string> tempPasswd)
    {
        int index = index = rand.Next(0, 25);
        int tries = -1;

        while (numGerados.Contains(index) | tries < maxTries) { index = rand.Next(0, 25); }
        numGerados.Add(index);
        tempPasswd.Add($"{caracteres[index].ToUpper()}");
    }

    private static void getLower(Random rand, int maxTries, ref List<int> numGerados, ref List<string> tempPasswd)
    {
        int index = rand.Next(0, 25);
        int tries = -1;

        while (numGerados.Contains(index) | tries < maxTries) { index = rand.Next(0, 25); tries++; }
        numGerados.Add(index);
        tempPasswd.Add($"{caracteres[index].ToLower()}"); ;
    }

    private static void getSpecial(Random rand, int maxTries, ref List<int> numGerados, ref List<string> tempPasswd)
    {
        int index = rand.Next(36, 50);
        int tries = -1;

        while (numGerados.Contains(index) | tries < maxTries) { index = rand.Next(36, 50); tries++; }
        numGerados.Add(index);
        tempPasswd.Add($"{caracteres[index]}");
    }

    private static void getNumber(Random rand, int maxTries, ref List<int> numGerados, ref List<string> tempPasswd)
    {
        int index = rand.Next(26, 35);
        int tries = -1;

        while (numGerados.Contains(index) | tries < maxTries) { index = rand.Next(26, 35); tries++; }
        numGerados.Add(index);
        tempPasswd.Add($"{caracteres[index]}");
    }

    private static void getAny(Random rand, int maxTries, ref List<int> numGerados, ref List<string> tempPasswd)
    {
        int index = rand.Next(0, 50);
        int tries = -1;

        while (numGerados.Contains(index) | tries < maxTries) { index = rand.Next(0, 50); tries++; }
        tempPasswd.Add($"{caracteres[index].ToUpper()}");    //  any
        numGerados.Add(index);
    }

    #endregion Métodos privados de apoio
}
