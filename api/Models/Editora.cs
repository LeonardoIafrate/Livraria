using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Editora
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        public List<Livro> Livros {get; set;} = new();
    }
}