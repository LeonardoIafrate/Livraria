using api.Data;
using api.Exceptions;
using api.Interfaces;
using api.Repository;
using api.Services;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Microsoft.AspNetCore.Identity;
using api.Helpers;
using api.Settings;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false)))
    .AddRespostaDeValidacao();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IEditoraRepository, EditoraRepository>();
builder.Services.AddScoped<IEditoraService, EditoraService>();
builder.Services.AddScoped<ILivroRepository, LivroRepository>();
builder.Services.AddScoped<ILivroService, LivroService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<IEstoqueRepository, EstoqueRepository>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEnderecoRepository, EnderecoRepository>();
builder.Services.AddScoped<IEnderecoService, EnderecoService>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

builder.Services.AddScoped<DbInitializer>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

if (Encoding.UTF8.GetByteCount(jwt.Chave) < 32)
{
    throw new InvalidOperationException(
        "A configuração Jwt:Chave está ausente ou tem menos de 32 bytes. " +
        "Defina com: dotnet user-secrets set \"Jwt:Chave\" \"<valor>\"");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidIssuer = jwt.Issuer,

          ValidateAudience = true,
          ValidAudience = jwt.Audience,

          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),

          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllers();
builder.Services.AddControllers().AddRespostaDeValidacao();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "LibCore API", Version = "v1" });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta forma: Bearer {seu_token}"
    });
    option.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();

        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await initializer.InitializeAsync();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("CatalogoInicial");

        await CatalogoInicial.SemearAsync(dbContext, logger);
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

public partial class Program { }