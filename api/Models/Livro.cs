using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Livro
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Sinopse { get; set; }
        public int? AnoLancamento { get; set; }
        public string Isbn{ get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int? NumeroPaginas { get; set; }
        public string Idioma { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public int EditoraId { get; set; }
        public Editora Editora { get; set; } = null!;
    }
}