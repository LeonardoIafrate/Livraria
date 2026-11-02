namespace api.Settings
{
    public class JwtSettings
    {
        public string Chave { get; set; } = string.Empty;
        public string Issuer { get; set; } = "libcore-api";
        public string Audience { get; set; } = "libcore-clientes";
        public int ExpiracaoEmMinutos { get; set; } = 60;
    }
}