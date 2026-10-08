using GerenciadorDeTarefas.Controllers;
using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Enums;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDeTarefas.Tests;

public class TarefaControllerTests
{
    [Fact]
    public async Task ObterPorUsuario_DeveRetornarTarefasDoUsuario()
    {
        var usuarioId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();

        var service = new FakeTarefaService();
        service.Tarefas.Add(CriarTarefa(usuarioId));
        service.Tarefas.Add(CriarTarefa(usuarioId));
        service.Tarefas.Add(CriarTarefa(outroUsuarioId));

        var controller = new TarefaController(service);

        var resultado = await controller.ObterPorUsuario(usuarioId);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var tarefas = Assert.IsAssignableFrom<IEnumerable<TarefaDto>>(
            okResult.Value
        );

        Assert.Equal(2, tarefas.Count());
    }

    [Fact]
    public async Task ObterPorUsuario_DeveRetornarListaVazia_QuandoNaoHouverTarefas()
    {
        var controller = new TarefaController(
            new FakeTarefaService()
        );

        var resultado = await controller.ObterPorUsuario(Guid.NewGuid());

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var tarefas = Assert.IsAssignableFrom<IEnumerable<TarefaDto>>(
            okResult.Value
        );

        Assert.Empty(tarefas);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarTarefaDoUsuario()
    {
        var usuarioId = Guid.NewGuid();
        var tarefa = CriarTarefa(usuarioId);

        var service = new FakeTarefaService();
        service.Tarefas.Add(tarefa);

        var controller = new TarefaController(service);

        var resultado = await controller.ObterPorId(
            usuarioId,
            tarefa.Id
        );

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var resposta = Assert.IsType<TarefaDto>(okResult.Value);

        Assert.Equal(tarefa.Id, resposta.Id);
        Assert.Equal(tarefa.Titulo, resposta.Titulo);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoTarefaNaoExistir()
    {
        var controller = new TarefaController(
            new FakeTarefaService()
        );

        var resultado = await controller.ObterPorId(
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoTarefaForDeOutroUsuario()
    {
        var donoId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();
        var tarefa = CriarTarefa(donoId);

        var service = new FakeTarefaService();
        service.Tarefas.Add(tarefa);

        var controller = new TarefaController(service);

        var resultado = await controller.ObterPorId(
            outroUsuarioId,
            tarefa.Id
        );

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Criar_DeveCadastrarTarefaComDadosValidos()
    {
        var usuarioId = Guid.NewGuid();
        var service = new FakeTarefaService();
        var controller = new TarefaController(service);

        var dto = CriarTarefaDtoValido();

        var resultado = await controller.Criar(usuarioId, dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(
            resultado
        );

        var resposta = Assert.IsType<TarefaDto>(
            createdResult.Value
        );

        Assert.Equal(dto.Titulo, resposta.Titulo);
        Assert.Equal(dto.Descricao, resposta.Descricao);

        Assert.NotNull(service.UltimaTarefaCriada);
        Assert.Equal(
            usuarioId,
            service.UltimaTarefaCriada.UsuarioId
        );
    }

    [Fact]
    public async Task Criar_DevePropagarExcecao_QuandoUsuarioNaoExistir()
    {
        var service = new FakeTarefaService
        {
            ExcecaoCriar = new InvalidOperationException(
                "Usuário não encontrado."
            )
        };

        var controller = new TarefaController(service);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.Criar(
                Guid.NewGuid(),
                CriarTarefaDtoValido()
            )
        );

        Assert.Equal("Usuário não encontrado.", excecao.Message);
    }

    [Fact]
    public async Task Criar_DevePropagarExcecao_QuandoTituloForDuplicado()
    {
        var service = new FakeTarefaService
        {
            ExcecaoCriar = new InvalidOperationException(
                "Tarefa já cadastrada para este usuário."
            )
        };

        var controller = new TarefaController(service);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.Criar(
                Guid.NewGuid(),
                CriarTarefaDtoValido()
            )
        );

        Assert.Equal(
            "Tarefa já cadastrada para este usuário.",
            excecao.Message
        );
    }

    [Fact]
    public async Task Criar_DeveRetornar201Created()
    {
        var controller = new TarefaController(
            new FakeTarefaService()
        );

        var resultado = await controller.Criar(
            Guid.NewGuid(),
            CriarTarefaDtoValido()
        );

        var createdResult = Assert.IsType<CreatedAtActionResult>(
            resultado
        );

        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(
            nameof(TarefaController.ObterPorId),
            createdResult.ActionName
        );
    }

    [Fact]
    public async Task Atualizar_DeveAtualizarTarefaDoUsuario()
    {
        var usuarioId = Guid.NewGuid();
        var tarefa = CriarTarefa(usuarioId);

        var service = new FakeTarefaService();
        service.Tarefas.Add(tarefa);

        var controller = new TarefaController(service);

        var dto = new AtualizarTarefaDto
        {
            Titulo = "Título atualizado",
            Descricao = "Descrição atualizada",
            DataDeVencimento = DateTime.Now.AddDays(10)
        };

        var resultado = await controller.Atualizar(
            usuarioId,
            tarefa.Id,
            dto
        );

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var resposta = Assert.IsType<TarefaDto>(okResult.Value);

        Assert.Equal("Título atualizado", resposta.Titulo);
        Assert.Equal("Descrição atualizada", resposta.Descricao);
    }

    [Fact]
    public async Task Atualizar_DevePropagarExcecao_QuandoTarefaForDeOutroUsuario()
    {
        var service = new FakeTarefaService
        {
            ExcecaoAtualizar = new InvalidOperationException(
                "Tarefa não pertence ao usuário."
            )
        };

        var controller = new TarefaController(service);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.Atualizar(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CriarAtualizacaoDtoValida()
            )
        );

        Assert.Equal(
            "Tarefa não pertence ao usuário.",
            excecao.Message
        );
    }

    [Fact]
    public async Task Concluir_DeveConcluirTarefaDoUsuario()
    {
        var usuarioId = Guid.NewGuid();
        var tarefa = CriarTarefa(usuarioId);

        var service = new FakeTarefaService();
        service.Tarefas.Add(tarefa);

        var controller = new TarefaController(service);

        await controller.Concluir(usuarioId, tarefa.Id);

        Assert.True(service.ConcluirFoiChamado);
        Assert.Equal(usuarioId, service.UltimoUsuarioId);
        Assert.Equal(tarefa.Id, service.UltimaTarefaId);
    }

    [Fact]
    public async Task Concluir_DevePropagarExcecao_QuandoTarefaForDeOutroUsuario()
    {
        var service = new FakeTarefaService
        {
            ExcecaoConcluir = new InvalidOperationException(
                "Tarefa não pertence ao usuário."
            )
        };

        var controller = new TarefaController(service);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.Concluir(
                Guid.NewGuid(),
                Guid.NewGuid()
            )
        );

        Assert.Equal(
            "Tarefa não pertence ao usuário.",
            excecao.Message
        );
    }

    [Fact]
    public async Task Concluir_DeveAlterarStatusParaConcluida()
    {
        var usuarioId = Guid.NewGuid();
        var tarefa = CriarTarefa(usuarioId);
        tarefa.Status = StatusTarefa.Pendente;

        var service = new FakeTarefaService();
        service.Tarefas.Add(tarefa);

        var controller = new TarefaController(service);

        await controller.Concluir(usuarioId, tarefa.Id);

        Assert.Equal(
            StatusTarefa.Concluida,
            tarefa.Status
        );
    }

    [Fact]
    public async Task Remover_DeveRemoverTarefaDoUsuario()
    {
        var usuarioId = Guid.NewGuid();
        var tarefa = CriarTarefa(usuarioId);

        var service = new FakeTarefaService();
        service.Tarefas.Add(tarefa);

        var controller = new TarefaController(service);

        await controller.Remover(usuarioId, tarefa.Id);

        Assert.DoesNotContain(tarefa, service.Tarefas);
    }

    [Fact]
    public async Task Remover_DevePropagarExcecao_QuandoTarefaForDeOutroUsuario()
    {
        var service = new FakeTarefaService
        {
            ExcecaoRemover = new InvalidOperationException(
                "Tarefa não pertence ao usuário."
            )
        };

        var controller = new TarefaController(service);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.Remover(
                Guid.NewGuid(),
                Guid.NewGuid()
            )
        );

        Assert.Equal(
            "Tarefa não pertence ao usuário.",
            excecao.Message
        );
    }

    [Fact]
    public async Task Concluir_DeveRetornar204NoContent()
    {
        var controller = new TarefaController(
            new FakeTarefaService()
        );

        var resultado = await controller.Concluir(
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        var noContentResult = Assert.IsType<NoContentResult>(
            resultado
        );

        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task Remover_DeveRetornar204NoContent()
    {
        var controller = new TarefaController(
            new FakeTarefaService()
        );

        var resultado = await controller.Remover(
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        var noContentResult = Assert.IsType<NoContentResult>(
            resultado
        );

        Assert.Equal(204, noContentResult.StatusCode);
    }

    private static Tarefa CriarTarefa(Guid usuarioId)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Tarefa de teste",
            Descricao = "Descrição da tarefa",
            DataDeVencimento = DateTime.Now.AddDays(5),
            Status = StatusTarefa.Pendente,
            UsuarioId = usuarioId
        };
    }

    private static CriarTarefaDto CriarTarefaDtoValido()
    {
        return new CriarTarefaDto
        {
            Titulo = "Nova tarefa",
            Descricao = "Descrição da nova tarefa",
            DataDeVencimento = DateTime.Now.AddDays(5)
        };
    }

    private static AtualizarTarefaDto CriarAtualizacaoDtoValida()
    {
        return new AtualizarTarefaDto
        {
            Titulo = "Tarefa atualizada",
            Descricao = "Descrição atualizada",
            DataDeVencimento = DateTime.Now.AddDays(10)
        };
    }

    private class FakeTarefaService : ITarefaService
    {
        public List<Tarefa> Tarefas { get; } = [];

        public Exception? ExcecaoCriar { get; set; }
        public Exception? ExcecaoAtualizar { get; set; }
        public Exception? ExcecaoConcluir { get; set; }
        public Exception? ExcecaoRemover { get; set; }

        public Tarefa? UltimaTarefaCriada { get; set; }

        public bool ConcluirFoiChamado { get; set; }

        public Guid UltimoUsuarioId { get; set; }
        public Guid UltimaTarefaId { get; set; }

        public Task<IEnumerable<Tarefa>> ObterPorUsuarioAsync(
            Guid usuarioId)
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

        public Task<Tarefa> CriarAsync(
            Guid usuarioId,
            Tarefa tarefa)
        {
            if (ExcecaoCriar is not null)
            {
                throw ExcecaoCriar;
            }

            tarefa.Id = Guid.NewGuid();
            tarefa.UsuarioId = usuarioId;
            tarefa.Status = StatusTarefa.Pendente;

            Tarefas.Add(tarefa);
            UltimaTarefaCriada = tarefa;

            return Task.FromResult(tarefa);
        }

        public Task<Tarefa> AtualizarAsync(
            Guid usuarioId,
            Guid id,
            Tarefa tarefa)
        {
            if (ExcecaoAtualizar is not null)
            {
                throw ExcecaoAtualizar;
            }

            var existente = Tarefas
                .FirstOrDefault(tarefa => tarefa.Id == id);

            if (existente is not null)
            {
                existente.Titulo = tarefa.Titulo;
                existente.Descricao = tarefa.Descricao;
                existente.DataDeVencimento =
                    tarefa.DataDeVencimento;

                return Task.FromResult(existente);
            }

            tarefa.Id = id;
            tarefa.UsuarioId = usuarioId;

            return Task.FromResult(tarefa);
        }

        public Task ConcluirAsync(Guid usuarioId, Guid id)
        {
            if (ExcecaoConcluir is not null)
            {
                throw ExcecaoConcluir;
            }

            ConcluirFoiChamado = true;
            UltimoUsuarioId = usuarioId;
            UltimaTarefaId = id;

            var tarefa = Tarefas
                .FirstOrDefault(tarefa => tarefa.Id == id);

            if (tarefa is not null)
            {
                tarefa.Status = StatusTarefa.Concluida;
            }

            return Task.CompletedTask;
        }

        public Task RemoverAsync(Guid usuarioId, Guid id)
        {
            if (ExcecaoRemover is not null)
            {
                throw ExcecaoRemover;
            }

            var tarefa = Tarefas
                .FirstOrDefault(tarefa => tarefa.Id == id);

            if (tarefa is not null)
            {
                Tarefas.Remove(tarefa);
            }

            return Task.CompletedTask;
        }
    }
}