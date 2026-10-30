namespace api.Tests.Integracao
{
    public static class CpfDeTeste
    {
        public static string Novo()
        {
            while (true)
            {
                var digitos = new int[11];

                for (var i =0; i < 9; i++)
                    digitos[i] = Random.Shared.Next(0, 10);

                digitos[9] = CalcularDigitos(digitos, 9);
                digitos[10] = CalcularDigitos(digitos, 10);

                if(digitos.Distinct().Count() == 1)
                    continue;
                
                return string.Concat(digitos);
            }
        }

        private static int CalcularDigitos(int[] digitos, int quantidade)
        {
            var soma = 0;

            for(var i = 0; i < quantidade; i++)
                soma += digitos[i] * (quantidade + 1 - i);

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
}