namespace api.Dtos.Auth
{
    public class TokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEm { get; set; }
    }
}