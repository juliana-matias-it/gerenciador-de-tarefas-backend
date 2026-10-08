using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDeTarefas.Controllers;

[ApiController]
[Route("api/usuarios/{usuarioId:guid}/tarefas")]
public class TarefaController : ControllerBase
{
    private readonly ITarefaService _tarefaService;

    public TarefaController(ITarefaService tarefaService)
    {
        _tarefaService = tarefaService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterPorUsuario(Guid usuarioId)
    {
        var tarefas = await _tarefaService.ObterPorUsuarioAsync(usuarioId);

        var resposta = tarefas.Select(ParaDto);

        return Ok(resposta);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid usuarioId, Guid id)
    {
        var tarefa = await _tarefaService.ObterPorIdAsync(id);

        if (tarefa is null || tarefa.UsuarioId != usuarioId)
        {
            return NotFound();
        }

        return Ok(ParaDto(tarefa));
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        Guid usuarioId,
        CriarTarefaDto dto)
    {
        var tarefa = new Tarefa
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            DataDeVencimento = dto.DataDeVencimento
        };

        var tarefaCriada = await _tarefaService.CriarAsync(
            usuarioId,
            tarefa
        );

        return CreatedAtAction(
            nameof(ObterPorId),
            new
            {
                usuarioId,
                id = tarefaCriada.Id
            },
            ParaDto(tarefaCriada)
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid usuarioId,
        Guid id,
        AtualizarTarefaDto dto)
    {
        var tarefa = new Tarefa
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            DataDeVencimento = dto.DataDeVencimento
        };

        var tarefaAtualizada = await _tarefaService.AtualizarAsync(
            usuarioId,
            id,
            tarefa
        );

        return Ok(ParaDto(tarefaAtualizada));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Concluir(
        Guid usuarioId,
        Guid id)
    {
        await _tarefaService.ConcluirAsync(usuarioId, id);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(
        Guid usuarioId,
        Guid id)
    {
        await _tarefaService.RemoverAsync(usuarioId, id);

        return NoContent();
    }

    private static TarefaDto ParaDto(Tarefa tarefa)
    {
        return new TarefaDto
        {
            Id = tarefa.Id,
            Titulo = tarefa.Titulo,
            Descricao = tarefa.Descricao,
            DataDeVencimento = tarefa.DataDeVencimento,
            Status = tarefa.Status
        };
    }
}