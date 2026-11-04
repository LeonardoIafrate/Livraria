using api.Exceptions;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class DbInitializer
    {
        private readonly AppDbContext _contexto;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IEnderecoRepository _enderecoRepo;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly IConfiguration _configuration;

        public DbInitializer(
            AppDbContext contexto, 
            IUsuarioRepository usuarioRepo, 
            IEnderecoRepository enderecoRepo,
            IPasswordHasher<Usuario> passwordHasher, 
            IConfiguration configuration)
        {
            _contexto = contexto;
            _usuarioRepo = usuarioRepo;
            _enderecoRepo = enderecoRepo;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {

            await _contexto.Database.MigrateAsync();

            var nome = GetRequired("BootstrapAdmin:Name").Trim().ToLowerInvariant();
            var email = GetRequired("BootstrapAdmin:Email").Trim().ToLowerInvariant();
            var senha = GetRequired("BootstrapAdmin:Password");
            var cpf = CpfHellper.SomenteDigitos(GetRequired("BootstrapAdmin:Cpf"));

            if(!CpfHellper.EhValido(cpf))
                throw new InvalidOperationException("O CPF configurado para o administrador é inválido.");

            var usuarioExistente = await _usuarioRepo.GetByEmailAsync(email);

            if(usuarioExistente != null)
            {
                if(usuarioExistente.Perfil != PerfilUsuario.Admin)
                    throw new ConflictException("Este e-mail já está em uso");

                if(!usuarioExistente.Ativo)
                    throw new ConflictException("O usuário precisa estar ativo");


                var enderecos = await _enderecoRepo.GetByUsuarioIdAsync(usuarioExistente.Id);

                if(enderecos.Count == 0)
                {
                    var novoEndereco = CriarEnderecoInicial();
                    
                    usuarioExistente.Enderecos.Add(novoEndereco);
                    await _enderecoRepo.DefinirPrincipalAsync(novoEndereco.Id);
                    await _contexto.SaveChangesAsync();

                    return;
                }


                await _contexto.SaveChangesAsync();
                
                return;
            }          

            var cpfExistente = await _usuarioRepo.GetByCpfAsync(cpf);

            if(cpfExistente != null)
                throw new ConflictException("Esse CPF já está em uso.");

            var admin = new Usuario
            {
              Nome = nome,
              Email = email,
              Cpf = cpf,
              Perfil = PerfilUsuario.Admin,
              Ativo = true,
              Enderecos = new List<Endereco>{CriarEnderecoInicial()}  
            };

            admin.SenhaHash = _passwordHasher.HashPassword(admin, senha);

            _contexto.Usuario.Add(admin);
            await _contexto.SaveChangesAsync();
        }



        private static Endereco CriarEnderecoInicial()
        {
            return new Endereco
            {
                Cep = "01001000",
                Rua = "Praça da Sé",
                Numero = "100",
                Complemento = "Endereço de demonstração",
                Bairro = "Sé",
                Cidade = "São Paulo",
                Uf = "SP",

                Ativo = true
            };
        }

        private  string GetRequired(string key)
        {
            var value = _configuration[key];

            if(string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"A configuração obrigatória '{key}' não foi definida.");

            return value;
        }
    }
}