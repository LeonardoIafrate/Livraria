namespace api.Helpers
{
    public static class PoliticaDeLogin
    {
        public const int MaxTentativas = 5;
        public static readonly TimeSpan DuracaoBloqueio = TimeSpan.FromMinutes(15);
    }
}