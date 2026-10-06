namespace api.Helpers
{
    public static class StringExtensions
    {
        public static string? FormatarOpcional(this string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        public static string FormatarIsbn(this string isbn)
        {
            return isbn.Trim().ToUpperInvariant();
        }
    }
}