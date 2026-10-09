using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class DataNascimentoAttribute : ValidationAttribute
    {
        private static readonly DateTime DataMinima = new(1900, 1, 1);

        public DataNascimentoAttribute(): base("Data de nascimento inválida")
        {
        }

        public override bool IsValid(object? value)
        {
            if (value is not DateTime data)
                return false;
            
            return data.Date >= DataMinima && data.Date <= DateTime.Today;
        }
    }
}