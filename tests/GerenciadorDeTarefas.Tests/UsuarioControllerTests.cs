using System.Text.Json;
using GerenciadorDeTarefas.Controllers;
using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDeTarefas.Tests;

public class UsuarioControllerTests
{
    [Fact]
    public async Task Cadastrar_DeveCadastrarUsuarioComDadosValidos()
    {
        var service = new FakeUsuarioService();

        var usuarioCadastrado = CriarUsuario();
        service.UsuarioCadastrado = usuarioCadastrado;

        var controller = new UsuarioController(service);

        var dto = CriarCadastroDto();

        var resultado = await controller.Cadastrar(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado);
        var resposta = Assert.IsType<UsuarioDto>(createdResult.Value);

        Assert.Equal(usuarioCadastrado.Id, resposta.Id);
        Assert.Equal(usuarioCadastrado.Nome, resposta.Nome);
        Assert.Equal(usuarioCadastrado.Email, resposta.Email);
    }

    [Fact]
    public async Task Cadastrar_DevePropagarExcecao_QuandoEmailJaExistir()
    {
        var service = new FakeUsuarioService
        {
            ExcecaoCadastro = new InvalidOperationException(
                "E-mail já cadastrado."
            )
        };

        var controller = new UsuarioController(service);

        var dto = CriarCadastroDto();

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.Cadastrar(dto)
        );

        Assert.Equal("E-mail já cadastrado.", excecao.Message);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarUsuarioExistente()
    {
        var usuario = CriarUsuario();

        var service = new FakeUsuarioService
        {
            UsuarioPorId = usuario
        };

        var controller = new UsuarioController(service);

        var resultado = await controller.ObterPorId(usuario.Id);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var resposta = Assert.IsType<UsuarioDto>(okResult.Value);

        Assert.Equal(usuario.Id, resposta.Id);
        Assert.Equal(usuario.Nome, resposta.Nome);
        Assert.Equal(usuario.Email, resposta.Email);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoUsuarioNaoExistir()
    {
        var service = new FakeUsuarioService
        {
            UsuarioPorId = null
        };

        var controller = new UsuarioController(service);

        var resultado = await controller.ObterPorId(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Cadastrar_DeveRetornar201Created()
    {
        var usuario = CriarUsuario();

        var service = new FakeUsuarioService
        {
            UsuarioCadastrado = usuario
        };

        var controller = new UsuarioController(service);

        var resultado = await controller.Cadastrar(CriarCadastroDto());

        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado);

        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(
            nameof(UsuarioController.ObterPorId),
            createdResult.ActionName
        );
    }

    [Fact]
    public async Task ObterPorId_DeveRetornar404_QuandoUsuarioNaoExistir()
    {
        var controller = new UsuarioController(
            new FakeUsuarioService()
        );

        var resultado = await controller.ObterPorId(Guid.NewGuid());

        var notFoundResult = Assert.IsType<NotFoundResult>(resultado);

        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task RespostaNaoDeveConterSenha()
    {
        var usuario = CriarUsuario();

        var service = new FakeUsuarioService
        {
            UsuarioPorId = usuario
        };

        var controller = new UsuarioController(service);

        var resultado = await controller.ObterPorId(usuario.Id);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var resposta = Assert.IsType<UsuarioDto>(okResult.Value);

        var json = JsonSerializer.Serialize(resposta);

        Assert.DoesNotContain(
            "senha",
            json.ToLowerInvariant()
        );
    }

    private static Usuario CriarUsuario()
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Usuário Teste",
            Email = "usuario@teste.com",
            Senha = "senha123"
        };
    }

    private static CadastrarUsuarioDto CriarCadastroDto()
    {
        return new CadastrarUsuarioDto
        {
            Nome = "Usuário Teste",
            Email = "usuario@teste.com",
            Senha = "senha123"
        };
    }

    private class FakeUsuarioService : IUsuarioService
    {
        public Usuario? UsuarioPorId { get; set; }

        public Usuario? UsuarioPorEmail { get; set; }

        public Usuario? UsuarioCadastrado { get; set; }

        public Exception? ExcecaoCadastro { get; set; }

        public Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            return Task.FromResult(UsuarioPorId);
        }

        public Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return Task.FromResult(UsuarioPorEmail);
        }

        public Task<Usuario> CadastrarAsync(Usuario usuario)
        {
            if (ExcecaoCadastro is not null)
            {
                throw ExcecaoCadastro;
            }

            return Task.FromResult(
                UsuarioCadastrado ?? usuario
            );
        }
    }
}