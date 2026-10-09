using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    public static class CpfHellper
    {
        public static string SomenteDigitos(string? valor)
        {
            return new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
        }

        public static bool EhValido(string? cpf)
        {
            var digitos = SomenteDigitos(cpf);

            if (digitos.Length != 11)
                return false;

            if(digitos.Distinct().Count() == 1)
                return false;

            return CalcularDigito(digitos, 9) == digitos[9] - '0' 
                && CalcularDigito(digitos, 10) == digitos[10] - '0';
        }

        private static int CalcularDigito(string digitos, int quantidade)
        {
            var soma = 0;

            for(var i =0; i < quantidade; i++)
                soma += (digitos[i] - '0') * (quantidade + 1 - i);

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
}