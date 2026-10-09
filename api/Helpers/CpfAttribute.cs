using System.ComponentModel.DataAnnotations;

namespace api.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class CpfAttribute : ValidationAttribute
    {
        public CpfAttribute() : base("Cpf inválido.")
        {
        }

        public override bool IsValid(object? value)
        {
            if(value is null)
                return true;

            return value is string texto && CpfHellper.EhValido(texto);
        }
    }
}