using GerenciadorDeTarefas.Enums;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Repositories.Interfaces;
using GerenciadorDeTarefas.Services;

namespace GerenciadorDeTarefas.Tests;

public class TarefaServiceTests
{
    [Fact]
    public async Task ObterPorUsuarioAsync_DeveRetornarTarefasDoUsuario()
    {
        var usuario = CriarUsuario();
        var tarefa1 = CriarTarefa(usuario);
        var tarefa2 = CriarTarefa(usuario);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.AddRange([tarefa1, tarefa2]);

        var service = CriarService(tarefaRepository);

        var resultado = await service.ObterPorUsuarioAsync(usuario.Id);

        Assert.Equal(2, resultado.Count());
    }

    [Fact]
    public async Task ObterPorUsuarioAsync_DeveRetornarListaVazia_QuandoUsuarioNaoTiverTarefas()
    {
        var tarefaRepository = new FakeTarefaRepository();
        var service = CriarService(tarefaRepository);

        var resultado = await service.ObterPorUsuarioAsync(Guid.NewGuid());

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarTarefa_QuandoExistir()
    {
        var usuario = CriarUsuario();
        var tarefa = CriarTarefa(usuario);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefa);

        var service = CriarService(tarefaRepository);

        var resultado = await service.ObterPorIdAsync(tarefa.Id);

        Assert.NotNull(resultado);
        Assert.Equal(tarefa.Id, resultado.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExistir()
    {
        var service = CriarService(new FakeTarefaRepository());

        var resultado = await service.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarTarefaComDadosValidos()
    {
        var usuario = CriarUsuario();
        var tarefaRepository = new FakeTarefaRepository();
        var usuarioRepository = new FakeUsuarioRepository();
        usuarioRepository.Usuarios.Add(usuario);

        var service = new TarefaService(tarefaRepository, usuarioRepository);
        var tarefa = CriarTarefaSemUsuario();

        var resultado = await service.CriarAsync(usuario.Id, tarefa);

        Assert.Contains(resultado, tarefaRepository.Tarefas);
        Assert.Equal(usuario.Id, resultado.UsuarioId);
        Assert.Equal(usuario, resultado.Usuario);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoUsuarioNaoExistir()
    {
        var service = CriarService(new FakeTarefaRepository());
        var tarefa = CriarTarefaSemUsuario();

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CriarAsync(Guid.NewGuid(), tarefa)
        );

        Assert.Equal("Usuário não encontrado.", excecao.Message);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoTituloJaExistirParaUsuario()
    {
        var usuario = CriarUsuario();
        var tarefaExistente = CriarTarefa(usuario);
        tarefaExistente.Titulo = "Estudar C#";

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefaExistente);

        var usuarioRepository = new FakeUsuarioRepository();
        usuarioRepository.Usuarios.Add(usuario);

        var service = new TarefaService(tarefaRepository, usuarioRepository);

        var novaTarefa = CriarTarefaSemUsuario();
        novaTarefa.Titulo = "Estudar C#";

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CriarAsync(usuario.Id, novaTarefa)
        );

        Assert.Equal(
            "Tarefa já cadastrada para este usuário.",
            excecao.Message
        );
    }

    [Fact]
    public async Task CriarAsync_DeveCriarTarefaComStatusPendente()
    {
        var usuario = CriarUsuario();
        var tarefaRepository = new FakeTarefaRepository();
        var usuarioRepository = new FakeUsuarioRepository();
        usuarioRepository.Usuarios.Add(usuario);

        var service = new TarefaService(tarefaRepository, usuarioRepository);

        var tarefa = CriarTarefaSemUsuario();
        tarefa.Status = StatusTarefa.Concluida;

        var resultado = await service.CriarAsync(usuario.Id, tarefa);

        Assert.Equal(StatusTarefa.Pendente, resultado.Status);
    }

    [Fact]
    public async Task CriarAsync_DeveAceitarDataDeVencimentoFutura()
    {
        var usuario = CriarUsuario();
        var tarefaRepository = new FakeTarefaRepository();
        var usuarioRepository = new FakeUsuarioRepository();
        usuarioRepository.Usuarios.Add(usuario);

        var service = new TarefaService(tarefaRepository, usuarioRepository);

        var tarefa = CriarTarefaSemUsuario();
        tarefa.DataDeVencimento = DateTime.Now.AddDays(1);

        var resultado = await service.CriarAsync(usuario.Id, tarefa);

        Assert.Equal(tarefa.DataDeVencimento, resultado.DataDeVencimento);
    }

    [Fact]
    public async Task CriarAsync_DeveRejeitarDataDeVencimentoInvalida()
    {
        var usuario = CriarUsuario();

        var usuarioRepository = new FakeUsuarioRepository();
        usuarioRepository.Usuarios.Add(usuario);

        var service = new TarefaService(
            new FakeTarefaRepository(),
            usuarioRepository
        );

        var tarefa = CriarTarefaSemUsuario();
        tarefa.DataDeVencimento = DateTime.Now.AddDays(-1);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CriarAsync(usuario.Id, tarefa)
        );

        Assert.Equal(
            "A data de vencimento deve ser posterior à data atual.",
            excecao.Message
        );
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarTarefaPertencenteAoUsuario()
    {
        var usuario = CriarUsuario();
        var tarefaExistente = CriarTarefa(usuario);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefaExistente);

        var service = CriarService(tarefaRepository);

        var novosDados = CriarTarefaSemUsuario();
        novosDados.Titulo = "Título atualizado";
        novosDados.Descricao = "Descrição atualizada";
        novosDados.DataDeVencimento = DateTime.Now.AddDays(10);

        var resultado = await service.AtualizarAsync(
            usuario.Id,
            tarefaExistente.Id,
            novosDados
        );

        Assert.Equal("Título atualizado", resultado.Titulo);
        Assert.Equal("Descrição atualizada", resultado.Descricao);
        Assert.Equal(novosDados.DataDeVencimento, resultado.DataDeVencimento);
        Assert.Equal(usuario.Id, resultado.UsuarioId);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRejeitarDataDeVencimentoInvalida()
    {
        var usuario = CriarUsuario();
        var tarefa = CriarTarefa(usuario);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefa);

        var service = CriarService(tarefaRepository);

        var novosDados = CriarTarefaSemUsuario();
        novosDados.DataDeVencimento = DateTime.Now.AddDays(-1);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AtualizarAsync(usuario.Id, tarefa.Id, novosDados)
        );

        Assert.Equal(
            "A data de vencimento deve ser posterior à data atual.",
            excecao.Message
        );
    }

    [Fact]
    public async Task AtualizarAsync_DeveRejeitarTarefaDeOutroUsuario()
    {
        var dono = CriarUsuario();
        var outroUsuario = CriarUsuario();

        var tarefa = CriarTarefa(dono);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefa);

        var service = CriarService(tarefaRepository);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AtualizarAsync(
                outroUsuario.Id,
                tarefa.Id,
                CriarTarefaSemUsuario()
            )
        );

        Assert.Equal(
            "Tarefa não pertence ao usuário.",
            excecao.Message
        );
    }

    [Fact]
    public async Task ConcluirAsync_DeveConcluirTarefaPertencenteAoUsuario()
    {
        var usuario = CriarUsuario();
        var tarefa = CriarTarefa(usuario);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefa);

        var service = CriarService(tarefaRepository);

        await service.ConcluirAsync(usuario.Id, tarefa.Id);

        Assert.Equal(StatusTarefa.Concluida, tarefa.Status);
    }

    [Fact]
    public async Task ConcluirAsync_DeveAlterarStatusParaConcluida()
    {
        var usuario = CriarUsuario();
        var tarefa = CriarTarefa(usuario);
        tarefa.Status = StatusTarefa.Pendente;

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefa);

        var service = CriarService(tarefaRepository);

        await service.ConcluirAsync(usuario.Id, tarefa.Id);

        Assert.Equal(StatusTarefa.Concluida, tarefa.Status);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemoverTarefaPertencenteAoUsuario()
    {
        var usuario = CriarUsuario();
        var tarefa = CriarTarefa(usuario);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefa);

        var service = CriarService(tarefaRepository);

        await service.RemoverAsync(usuario.Id, tarefa.Id);

        Assert.DoesNotContain(tarefa, tarefaRepository.Tarefas);
    }

    [Fact]
    public async Task RemoverAsync_DeveRejeitarTarefaDeOutroUsuario()
    {
        var dono = CriarUsuario();
        var outroUsuario = CriarUsuario();

        var tarefa = CriarTarefa(dono);

        var tarefaRepository = new FakeTarefaRepository();
        tarefaRepository.Tarefas.Add(tarefa);

        var service = CriarService(tarefaRepository);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RemoverAsync(outroUsuario.Id, tarefa.Id)
        );

        Assert.Equal(
            "Tarefa não pertence ao usuário.",
            excecao.Message
        );
    }

    private static TarefaService CriarService(
        FakeTarefaRepository tarefaRepository)
    {
        return new TarefaService(
            tarefaRepository,
            new FakeUsuarioRepository()
        );
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

    private static Tarefa CriarTarefa(Usuario usuario)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = $"Tarefa {Guid.NewGuid()}",
            Descricao = "Descrição da tarefa",
            DataDeVencimento = DateTime.Now.AddDays(5),
            Status = StatusTarefa.Pendente,
            UsuarioId = usuario.Id,
            Usuario = usuario
        };
    }

    private static Tarefa CriarTarefaSemUsuario()
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = $"Tarefa {Guid.NewGuid()}",
            Descricao = "Descrição da tarefa",
            DataDeVencimento = DateTime.Now.AddDays(5)
        };
    }

    private class FakeTarefaRepository : ITarefaRepository
    {
        public List<Tarefa> Tarefas { get; } = [];

        public Task<IEnumerable<Tarefa>> ObterPorUsuarioIdAsync(Guid usuarioId)
        {
            var tarefas = Tarefas
                .Where(tarefa => tarefa.UsuarioId == usuarioId);

            return Task.FromResult(tarefas);
        }

        public Task<Tarefa?> ObterPorIdAsync(Guid id)
        {
            var tarefa = Tarefas
                .FirstOrDefault(tarefa => tarefa.Id == id);

            return Task.FromResult(tarefa);
        }

        public Task<bool> ExisteAsync(Guid usuarioId, string titulo)
        {
            var existe = Tarefas.Any(tarefa =>
                tarefa.UsuarioId == usuarioId &&
                tarefa.Titulo == titulo
            );

            return Task.FromResult(existe);
        }

        public Task AdicionarAsync(Tarefa tarefa)
        {
            Tarefas.Add(tarefa);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Tarefa tarefa)
        {
            return Task.CompletedTask;
        }

        public Task RemoverAsync(Tarefa tarefa)
        {
            Tarefas.Remove(tarefa);
            return Task.CompletedTask;
        }
    }

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = [];

        public Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            var usuario = Usuarios
                .FirstOrDefault(usuario => usuario.Id == id);

            return Task.FromResult(usuario);
        }

        public Task<Usuario?> ObterPorEmailAsync(string email)
        {
            var usuario = Usuarios
                .FirstOrDefault(usuario => usuario.Email == email);

            return Task.FromResult(usuario);
        }

        public Task AdicionarAsync(Usuario usuario)
        {
            Usuarios.Add(usuario);
            return Task.CompletedTask;
        }
    }
}