using System.Net;
using System.Text.Json;
using GerenciadorDeTarefas.Middleware;
using Microsoft.AspNetCore.Http;

namespace GerenciadorDeTarefas.Tests;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task DeveRetornar409_QuandoEmailJaEstiverCadastrado()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new InvalidOperationException("E-mail já cadastrado.")
        );

        Assert.Equal(
            (int)HttpStatusCode.Conflict,
            context.Response.StatusCode
        );

        Assert.Equal(
            "E-mail já cadastrado.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public async Task DeveRetornar404_QuandoUsuarioNaoForEncontrado()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new InvalidOperationException("Usuário não encontrado.")
        );

        Assert.Equal(
            (int)HttpStatusCode.NotFound,
            context.Response.StatusCode
        );

        Assert.Equal(
            "Usuário não encontrado.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public async Task DeveRetornar404_QuandoTarefaNaoForEncontrada()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new InvalidOperationException("Tarefa não encontrada.")
        );

        Assert.Equal(
            (int)HttpStatusCode.NotFound,
            context.Response.StatusCode
        );

        Assert.Equal(
            "Tarefa não encontrada.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public async Task DeveRetornar404_QuandoTarefaNaoPertencerAoUsuario()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new InvalidOperationException(
                "Tarefa não pertence ao usuário."
            )
        );

        Assert.Equal(
            (int)HttpStatusCode.NotFound,
            context.Response.StatusCode
        );

        Assert.Equal(
            "Tarefa não pertence ao usuário.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public async Task DeveRetornar409_QuandoTarefaForDuplicada()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new InvalidOperationException(
                "Tarefa já cadastrada para este usuário."
            )
        );

        Assert.Equal(
            (int)HttpStatusCode.Conflict,
            context.Response.StatusCode
        );

        Assert.Equal(
            "Tarefa já cadastrada para este usuário.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public async Task DeveRetornar409_QuandoDataDeVencimentoForInvalida()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new InvalidOperationException(
                "A data de vencimento deve ser posterior à data atual."
            )
        );

        Assert.Equal(
            (int)HttpStatusCode.Conflict,
            context.Response.StatusCode
        );

        Assert.Equal(
            "A data de vencimento deve ser posterior à data atual.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public async Task DeveTratarExcecaoNaoPrevista()
    {
        var (context, _) = await ExecutarMiddlewareAsync(
            new Exception("Erro inesperado")
        );

        Assert.Equal(
            (int)HttpStatusCode.InternalServerError,
            context.Response.StatusCode
        );
    }

    [Fact]
    public async Task DeveRetornar404ParaRecursoNaoEncontrado()
    {
        var (context, _) = await ExecutarMiddlewareAsync(
            new InvalidOperationException("Tarefa não encontrada.")
        );

        Assert.Equal(404, context.Response.StatusCode);
    }

    [Fact]
    public async Task DeveRetornar409ParaConflitoDeRegraDeNegocio()
    {
        var (context, _) = await ExecutarMiddlewareAsync(
            new InvalidOperationException("E-mail já cadastrado.")
        );

        Assert.Equal(409, context.Response.StatusCode);
    }

    [Fact]
    public async Task DeveRetornar500ParaExcecaoNaoPrevista()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new NullReferenceException(
                "Informação interna que não pode vazar"
            )
        );

        Assert.Equal(500, context.Response.StatusCode);

        Assert.Equal(
            "Ocorreu um erro interno no servidor.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public async Task DeveRetornarRespostaJsonPadronizada()
    {
        var (context, json) = await ExecutarMiddlewareAsync(
            new InvalidOperationException("E-mail já cadastrado.")
        );

        using var documento = JsonDocument.Parse(json);

        var raiz = documento.RootElement;

        Assert.True(
            raiz.TryGetProperty("statusCode", out var statusCode)
        );

        Assert.True(
            raiz.TryGetProperty("message", out var message)
        );

        Assert.Equal(409, statusCode.GetInt32());
        Assert.Equal(
            "E-mail já cadastrado.",
            message.GetString()
        );

        Assert.StartsWith(
            "application/json",
            context.Response.ContentType
        );
    }

    [Fact]
    public async Task NaoDeveExporInformacoesInternas()
    {
        const string mensagemInterna =
            "Connection string: senha-secreta-do-banco";

        var (_, json) = await ExecutarMiddlewareAsync(
            new Exception(mensagemInterna)
        );

        Assert.DoesNotContain(
            mensagemInterna,
            json
        );

        Assert.DoesNotContain(
            "senha-secreta-do-banco",
            json
        );

        Assert.Equal(
            "Ocorreu um erro interno no servidor.",
            ObterMensagem(json)
        );
    }

    [Fact]
    public void ControllersNaoDevemPossuirTryCatch()
    {
        var raizProjeto = EncontrarRaizProjeto();

        var pastaControllers = Path.Combine(
            raizProjeto,
            "Controllers"
        );

        var arquivos = Directory.GetFiles(
            pastaControllers,
            "*.cs"
        );

        Assert.NotEmpty(arquivos);

        foreach (var arquivo in arquivos)
        {
            var conteudo = File.ReadAllText(arquivo)
                .ToLowerInvariant();

            Assert.DoesNotContain("catch", conteudo);
        }
    }

    private static async Task<(DefaultHttpContext Context, string Json)>
        ExecutarMiddlewareAsync(Exception exception)
    {
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        RequestDelegate proximo = _ =>
            throw exception;

        var middleware = new ExceptionMiddleware(proximo);

        await middleware.InvokeAsync(context);

        var stream = (MemoryStream)context.Response.Body;

        var json = System.Text.Encoding.UTF8.GetString(
            stream.ToArray()
        );

        return (context, json);
    }

    private static string? ObterMensagem(string json)
    {
        using var documento = JsonDocument.Parse(json);

        return documento.RootElement
            .GetProperty("message")
            .GetString();
    }

    private static string EncontrarRaizProjeto()
    {
        var diretorio = new DirectoryInfo(
            AppContext.BaseDirectory
        );

        while (diretorio is not null)
        {
            var arquivoProjeto = Path.Combine(
                diretorio.FullName,
                "GerenciadorDeTarefas.csproj"
            );

            var pastaControllers = Path.Combine(
                diretorio.FullName,
                "Controllers"
            );

            if (
                File.Exists(arquivoProjeto) &&
                Directory.Exists(pastaControllers)
            )
            {
                return diretorio.FullName;
            }

            diretorio = diretorio.Parent;
        }

        throw new DirectoryNotFoundException(
            "Não foi possível localizar a raiz do projeto."
        );
    }
}