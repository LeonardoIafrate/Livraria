namespace api.Settings
{
    public class JwtSettings
    {
        public string Chave { get; set; } = string.Empty;
        public string Issuer { get; set; } = "livraria-api";
        public string Audience { get; set; } = "livraria-clientes";
        public int ExpiracaoEmMinutos { get; set; } = 60;
    }
}