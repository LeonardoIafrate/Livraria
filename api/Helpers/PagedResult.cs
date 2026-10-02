namespace api.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Itens { get; }
        public int Total { get; }
        public int Pagina { get; }
        public int TamanhoPagina { get; }
        public int TotalPaginas => (int)Math.Ceiling(Total / (double)TamanhoPagina);

        public PagedResult(List<T> itens, int total, int pagina, int tamanhoPagina)
        {
            Itens = itens;
            Total = total;
            Pagina = pagina;
            TamanhoPagina = tamanhoPagina;
        }

        // Converte os itens (por exemplo, de entidade para DTO) mantendo os dados de paginação
        public PagedResult<TOut> Map<TOut>(Func<T, TOut> converter)
        {
            return new PagedResult<TOut>(
                Itens.Select(converter).ToList(),
                Total,
                Pagina,
                TamanhoPagina);
        }
    }
}