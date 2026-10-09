using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Repositories.Interfaces;
using GerenciadorDeTarefas.Services;

namespace GerenciadorDeTarefas.Tests;

public class UsuarioServiceTests
{
    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarUsuario_QuandoUsuarioExistir()
    {
        var usuario = CriarUsuario();
        var repository = new FakeUsuarioRepository();
        repository.Usuarios.Add(usuario);

        var service = new UsuarioService(repository);

        var resultado = await service.ObterPorIdAsync(usuario.Id);

        Assert.NotNull(resultado);
        Assert.Equal(usuario.Id, resultado.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoUsuarioNaoExistir()
    {
        var repository = new FakeUsuarioRepository();
        var service = new UsuarioService(repository);

        var resultado = await service.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorEmailAsync_DeveRetornarUsuario_QuandoEmailExistir()
    {
        var usuario = CriarUsuario();
        var repository = new FakeUsuarioRepository();
        repository.Usuarios.Add(usuario);

        var service = new UsuarioService(repository);

        var resultado = await service.ObterPorEmailAsync(usuario.Email);

        Assert.NotNull(resultado);
        Assert.Equal(usuario.Email, resultado.Email);
    }

    [Fact]
    public async Task ObterPorEmailAsync_DeveRetornarNull_QuandoEmailNaoExistir()
    {
        var repository = new FakeUsuarioRepository();
        var service = new UsuarioService(repository);

        var resultado = await service.ObterPorEmailAsync("naoexiste@email.com");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task CadastrarAsync_DeveCadastrarUsuarioComDadosValidos()
    {
        var repository = new FakeUsuarioRepository();
        var service = new UsuarioService(repository);
        var usuario = CriarUsuario();

        var resultado = await service.CadastrarAsync(usuario);

        Assert.NotNull(resultado);
        Assert.Contains(usuario, repository.Usuarios);
    }

    [Fact]
    public async Task CadastrarAsync_NaoDeveCadastrarUsuarioComEmailDuplicado()
    {
        var usuarioExistente = CriarUsuario();

        var repository = new FakeUsuarioRepository();
        repository.Usuarios.Add(usuarioExistente);

        var service = new UsuarioService(repository);

        var novoUsuario = CriarUsuario();
        novoUsuario.Email = usuarioExistente.Email;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CadastrarAsync(novoUsuario)
        );

        Assert.Single(repository.Usuarios);
    }

    [Fact]
    public async Task CadastrarAsync_DeveLancarExcecaoComMensagemCorreta_QuandoEmailForDuplicado()
    {
        var usuarioExistente = CriarUsuario();

        var repository = new FakeUsuarioRepository();
        repository.Usuarios.Add(usuarioExistente);

        var service = new UsuarioService(repository);

        var novoUsuario = CriarUsuario();
        novoUsuario.Email = usuarioExistente.Email;

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CadastrarAsync(novoUsuario)
        );

        Assert.Equal("E-mail já cadastrado.", excecao.Message);
    }

    [Fact]
    public async Task CadastrarAsync_DeveGerarNovoGuidParaUsuario()
    {
        var repository = new FakeUsuarioRepository();
        var service = new UsuarioService(repository);

        var usuario = CriarUsuario();
        usuario.Id = Guid.Empty;

        await service.CadastrarAsync(usuario);

        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    private static Usuario CriarUsuario()
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Usuário Teste",
            Email = $"usuario-{Guid.NewGuid()}@teste.com",
            Senha = "123456"
        };
    }

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = [];

        public Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            var usuario = Usuarios.FirstOrDefault(usuario => usuario.Id == id);
            return Task.FromResult(usuario);
        }

        public Task<Usuario?> ObterPorEmailAsync(string email)
        {
            var usuario = Usuarios.FirstOrDefault(usuario => usuario.Email == email);
            return Task.FromResult(usuario);
        }

        public Task AdicionarAsync(Usuario usuario)
        {
            Usuarios.Add(usuario);
            return Task.CompletedTask;
        }
    }
}