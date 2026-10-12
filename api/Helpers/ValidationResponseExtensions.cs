using Microsoft.AspNetCore.Mvc;

namespace api.Helpers
{
    public static class ValidationResponseExtensions
    {
        public static IMvcBuilder AddRespostaDeValidacao(this IMvcBuilder builder)
        {
            return builder.ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var erros = new Dictionary<string, string[]>();

                    foreach (var (campo, entrada) in context.ModelState)
                    {
                        if (entrada is null || entrada.Errors.Count == 0)
                            continue;

                        var mensagens = entrada.Errors
                            .Select(e => string.IsNullOrEmpty(e.ErrorMessage)
                                ? e.Exception?.Message
                                : e.ErrorMessage)
                            .Where(m => !string.IsNullOrEmpty(m))
                            .Select(m => Traduzir(m!))
                            .Distinct()
                            .ToArray();

                        if (mensagens.Length > 0)
                            erros[LimparNomeDoCampo(campo)] = mensagens;
                    }

                    var temErroDeJson = context.ModelState.Keys.Any(k => k.StartsWith("$"));
                    if (temErroDeJson)
                        erros.Remove("dto");

                    var problema = new ValidationProblemDetails(erros)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Um ou mais erros de validação ocorreram.",
                        Instance = context.HttpContext.Request.Path
                    };
                    problema.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    return new BadRequestObjectResult(problema)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });
        }
        private static string LimparNomeDoCampo(string campo)
        {
            return campo.StartsWith("$.") ? campo[2..] : campo;
        }

        private static string Traduzir(string mensagem)
        {
            if (mensagem.Contains("could not be converted to System.DateTime")
                || mensagem.Contains("could not be converted to System.DateOnly"))
                return "Data inválida. Use o formato AAAA-MM-DD (ex.: 2005-06-26).";

            if (mensagem.Contains("could not be converted to"))
                return "Valor em formato inválido para este campo.";

            return mensagem;
        }
    }
}