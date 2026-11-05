using Gastra.Communication.Responses;
using Gastra.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gastra.Api.Filtros;

/// <summary>
/// Tratamento global de erros: converte exceções em respostas HTTP com o formato de
/// <see cref="ErroResponse"/>. Exceções inesperadas viram 500 sem expor detalhes internos
/// (a exceção completa vai só para o log técnico).
/// </summary>
public class FiltroExcecao(ILogger<FiltroExcecao> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is GastraException excecao)
        {
            context.Result = new ObjectResult(new ErroResponse(excecao.ObterErros()))
            {
                StatusCode = excecao.StatusCode,
            };
        }
        else
        {
            logger.LogError(context.Exception, "Erro inesperado em {Rota}", context.HttpContext.Request.Path);
            context.Result = new ObjectResult(new ErroResponse(MensagensErro.ErroInesperado))
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }

        context.ExceptionHandled = true;
    }
}
