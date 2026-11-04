using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public static class CatalogoInicial
    {
        public static async Task SemearAsync(
            AppDbContext contexto,
            ILogger logger,
            CancellationToken cancelamento = default)
        {
            if (await contexto.Set<Livro>().AnyAsync(cancelamento))
            {
                logger.LogInformation("Catálogo inicial ignorado: já existem livros cadastrados.");
                return;
            }

            var editoras = (await contexto.Set<Editora>().ToListAsync(cancelamento))
                .ToDictionary(e => e.Nome, StringComparer.OrdinalIgnoreCase);
            var categorias = (await contexto.Set<Categoria>().ToListAsync(cancelamento))
                .ToDictionary(c => c.Genero, StringComparer.OrdinalIgnoreCase);
            var autores = (await contexto.Set<Autor>().ToListAsync(cancelamento))
                .ToDictionary(a => a.Nome, StringComparer.OrdinalIgnoreCase);

            Editora ObterEditora(string nome) =>
                editoras.TryGetValue(nome, out var existente)
                    ? existente
                    : editoras[nome] = new Editora { Nome = nome };

            Categoria ObterCategoria(string genero) =>
                categorias.TryGetValue(genero, out var existente)
                    ? existente
                    : categorias[genero] = new Categoria { Genero = genero };

            Autor ObterAutor(string nome, string nacionalidade, string biografia) =>
                autores.TryGetValue(nome, out var existente)
                    ? existente
                    : autores[nome] = new Autor { Nome = nome, Nacionalidade = nacionalidade, Biografia = biografia };


            var aurora = ObterEditora("Editora Aurora");
            var horizonte = ObterEditora("Editora Horizonte");
            var cais = ObterEditora("Editora Cais");


            var romance = ObterCategoria("Romance");
            var classico = ObterCategoria("Clássico");
            var brasileira = ObterCategoria("Literatura Brasileira");
            var portuguesa = ObterCategoria("Literatura Portuguesa");
            var realismo = ObterCategoria("Realismo");
            var romantismo = ObterCategoria("Romantismo");
            var infantil = ObterCategoria("Infantil");
            var aventura = ObterCategoria("Aventura");

            var machado = ObterAutor("Machado de Assis", "Brasileira",
                "Escritor carioca (1839–1908), fundador e primeiro presidente da Academia Brasileira de Letras. " +
                "Mestre da ironia e da análise psicológica, é considerado o maior nome da literatura brasileira.");

            var aluisio = ObterAutor("Aluísio Azevedo", "Brasileira",
                "Escritor maranhense (1857–1913), principal nome do Naturalismo no Brasil. " +
                "Retratou a vida urbana e as desigualdades sociais do século XIX.");

            var alencar = ObterAutor("José de Alencar", "Brasileira",
                "Escritor cearense (1829–1877), figura central do Romantismo brasileiro. " +
                "Dedicou-se a construir uma literatura de identidade nacional.");

            var lima = ObterAutor("Lima Barreto", "Brasileira",
                "Escritor carioca (1881–1922), crítico agudo da sociedade da Primeira República. " +
                "Retratou os subúrbios do Rio de Janeiro e o preconceito racial.");

            var graciliano = ObterAutor("Graciliano Ramos", "Brasileira",
                "Escritor alagoano (1892–1953), de prosa seca e precisa. " +
                "É um dos maiores nomes do Modernismo e da literatura regionalista brasileira.");

            var eca = ObterAutor("Eça de Queirós", "Portuguesa",
                "Escritor português (1845–1900), principal nome do Realismo em língua portuguesa. " +
                "Conhecido pela crítica à burguesia e à hipocrisia social.");

            var cervantes = ObterAutor("Miguel de Cervantes", "Espanhola",
                "Escritor espanhol (1547–1616), autor de Dom Quixote, obra fundadora do romance moderno.");

            var lobato = ObterAutor("Monteiro Lobato", "Brasileira",
                "Escritor paulista (1882–1948), pioneiro da literatura infantil brasileira. " +
                "Criou o universo do Sítio do Picapau Amarelo.");

            var almeida = ObterAutor("Manuel Antônio de Almeida", "Brasileira",
                "Escritor carioca (1831–1861), autor de um dos primeiros romances de costumes do Brasil, " +
                "publicado em capítulos nos jornais.");

            var macedo = ObterAutor("Joaquim Manuel de Macedo", "Brasileira",
                "Escritor fluminense (1820–1882), um dos fundadores do romance romântico brasileiro, " +
                "com histórias de amor ambientadas na sociedade carioca.");


            var livros = new List<Livro>
            {
                NovoLivro(1, "Dom Casmurro", machado, aurora, 1899, 39.90m, 256,
                    "Bentinho, já maduro, revive a juventude e o grande amor por Capitu enquanto tenta responder a uma " +
                    "pergunta que o corrói: teria sido traído?",
                    estoque: 25, romance, classico, brasileira),

                NovoLivro(2, "Memórias Póstumas de Brás Cubas", machado, aurora, 1881, 29.90m, 240,
                    "O defunto-autor Brás Cubas conta a própria vida com ironia e sem pressa, de além-túmulo, " +
                    "expondo as vaidades e as pequenezas da sociedade do seu tempo.",
                    estoque: 18, romance, classico, brasileira, realismo),

                NovoLivro(3, "Quincas Borba", machado, aurora, 1891, 34.90m, 280,
                    "Rubião herda uma fortuna e a filosofia excêntrica do Humanitismo, e vê o dinheiro e a " +
                    "ingenuidade o levarem à ruína no Rio de Janeiro.",
                    estoque: 12, romance, classico, brasileira, realismo),

                NovoLivro(4, "O Cortiço", aluisio, horizonte, 1890, 32.90m, 288,
                    "Num cortiço do Rio de Janeiro, a ambição de um comerciante português e a vida coletiva dos " +
                    "moradores mostram o lado cru da sociedade urbana do século XIX.",
                    estoque: 20, romance, classico, brasileira, realismo),

                NovoLivro(5, "Iracema", alencar, horizonte, 1865, 24.90m, 160,
                    "A jovem tabajara Iracema e o guerreiro português Martim vivem um amor impossível que, na " +
                    "leitura de Alencar, simboliza o nascimento do povo brasileiro.",
                    estoque: 30, romance, classico, brasileira, romantismo),

                NovoLivro(6, "O Guarani", alencar, horizonte, 1857, 36.90m, 352,
                    "No Brasil colonial, o índio Peri dedica a vida a proteger Ceci, filha de um fidalgo português, " +
                    "numa aventura de lealdade e amor impossível.",
                    estoque: 8, romance, classico, brasileira, romantismo, aventura),

                NovoLivro(7, "Triste Fim de Policarpo Quaresma", lima, cais, 1915, 37.90m, 296,
                    "Patriota fervoroso, o major Policarpo tenta transformar o Brasil com ideias grandiosas e acaba " +
                    "colidindo com a política e a burocracia reais.",
                    estoque: 15, romance, classico, brasileira),

                NovoLivro(8, "Vidas Secas", graciliano, cais, 1938, 29.90m, 176,
                    "Fabiano, Sinhá Vitória e os filhos fogem da seca do sertão nordestino numa travessia marcada " +
                    "pela miséria, pelo silêncio e pela dignidade.",
                    estoque: 0, romance, classico, brasileira),

                NovoLivro(9, "O Primo Basílio", eca, cais, 1878, 44.90m, 432,
                    "Luísa, entediada com a vida doméstica em Lisboa, reencontra o primo Basílio e se vê envolvida " +
                    "num adultério que lhe custará caro.",
                    estoque: 6, romance, classico, portuguesa, realismo),

                NovoLivro(10, "Dom Quixote", cervantes, aurora, 1605, 79.90m, 960,
                    "Um fidalgo enlouquecido pelos livros de cavalaria sai pelo mundo como cavaleiro andante, ao " +
                    "lado do fiel Sancho Pança, confundindo moinhos com gigantes.",
                    estoque: 10, romance, classico, aventura),

                NovoLivro(11, "Reinações de Narizinho", lobato, horizonte, 1931, 42.90m, 224,
                    "Narizinho, a boneca Emília e os moradores do Sítio do Picapau Amarelo vivem aventuras que " +
                    "misturam o cotidiano da fazenda e a fantasia.",
                    estoque: 22, infantil, brasileira, aventura),

                NovoLivro(12, "Memórias de um Sargento de Milícias", almeida, cais, 1854, 27.90m, 224,
                    "As peripécias do malandro Leonardo no Rio de Janeiro do tempo do rei D. João VI, entre amores, " +
                    "trapaças e a perseguição do major Vidigal.",
                    estoque: 3, romance, classico, brasileira, romantismo),

                NovoLivro(13, "A Moreninha", macedo, aurora, 1844, 22.90m, 176,
                    "Durante um feriado numa ilha, o estudante Augusto aposta que nunca se apaixonará e se vê às " +
                    "voltas com a encantadora Carolina.",
                    estoque: 14, romance, classico, brasileira, romantismo)
            };

            contexto.Set<Livro>().AddRange(livros);
            await contexto.SaveChangesAsync(cancelamento);

            logger.LogInformation("Catálogo inicial criado: {Livros} livros, {Autores} autores e {Categorias} categorias.",
                livros.Count, autores.Count, categorias.Count);
        }

        private static Livro NovoLivro(
            int sequencial,
            string nome,
            Autor autor,
            Editora editora,
            int ano,
            decimal preco,
            int paginas,
            string sinopse,
            int estoque,
            params Categoria[] categorias)
        {
            var livro = new Livro
            {
                Nome = nome,
                Sinopse = sinopse,
                AnoLancamento = ano,
                Isbn = Isbn13(sequencial),
                Preco = preco,
                NumeroPaginas = paginas,
                Idioma = "Português",
                Editora = editora,
                Estoque = new Estoque { Quantidade = estoque }
            };

            livro.Autores.Add(autor);

            foreach (var categoria in categorias)
                livro.Categorias.Add(categoria);

            return livro;
        }

        private static string Isbn13(int sequencial)
        {
            var prefixo = $"978000000{sequencial:D3}"; 
            var soma = 0;

            for (var i = 0; i < prefixo.Length; i++)
                soma += (prefixo[i] - '0') * (i % 2 == 0 ? 1 : 3);

            var digitoVerificador = (10 - soma % 10) % 10;

            return prefixo + digitoVerificador;
        }
    }
}