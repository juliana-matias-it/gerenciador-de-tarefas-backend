using System.Net;

namespace GerenciadorDeTarefas.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            await TratarInvalidOperationExceptionAsync(context, ex);
        }
        catch (Exception)
        {
            await EscreverRespostaAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Ocorreu um erro interno no servidor."
            );
        }
    }

    private static async Task TratarInvalidOperationExceptionAsync(
        HttpContext context,
        InvalidOperationException exception)
    {
        var statusCode = exception.Message switch
        {
            "Usuário não encontrado." => HttpStatusCode.NotFound,
            "Tarefa não encontrada." => HttpStatusCode.NotFound,
            "Tarefa não pertence ao usuário." => HttpStatusCode.NotFound,

            _ => HttpStatusCode.Conflict
        };

        await EscreverRespostaAsync(
            context,
            statusCode,
            exception.Message
        );
    }

    private static async Task EscreverRespostaAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var resposta = new
        {
            statusCode = (int)statusCode,
            message
        };

        await context.Response.WriteAsJsonAsync(resposta);
    }
}