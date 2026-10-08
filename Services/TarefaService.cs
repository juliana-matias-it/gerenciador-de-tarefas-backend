using GerenciadorDeTarefas.Enums;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Repositories.Interfaces;
using GerenciadorDeTarefas.Services.Interfaces;

namespace GerenciadorDeTarefas.Services;

public class TarefaService : ITarefaService
{
    private readonly ITarefaRepository _tarefaRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public TarefaService(
        ITarefaRepository tarefaRepository,
        IUsuarioRepository usuarioRepository)
    {
        _tarefaRepository = tarefaRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IEnumerable<Tarefa>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _tarefaRepository.ObterPorUsuarioIdAsync(usuarioId);
    }

    public async Task<Tarefa?> ObterPorIdAsync(Guid id)
    {
        return await _tarefaRepository.ObterPorIdAsync(id);
    }

    public async Task<Tarefa> CriarAsync(Guid usuarioId, Tarefa tarefa)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);

        if (usuario is null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var tarefaDuplicada = await _tarefaRepository.ExisteAsync(
            usuarioId,
            tarefa.Titulo
        );

        if (tarefaDuplicada)
        {
            throw new InvalidOperationException(
                "Tarefa já cadastrada para este usuário."
            );
        }

        ValidarDataDeVencimento(tarefa.DataDeVencimento);

        tarefa.UsuarioId = usuarioId;
        tarefa.Usuario = usuario;
        tarefa.Status = StatusTarefa.Pendente;

        await _tarefaRepository.AdicionarAsync(tarefa);

        return tarefa;
    }

    public async Task<Tarefa> AtualizarAsync(
        Guid usuarioId,
        Guid id,
        Tarefa tarefa)
    {
        var tarefaExistente = await _tarefaRepository.ObterPorIdAsync(id);

        if (tarefaExistente is null)
        {
            throw new InvalidOperationException("Tarefa não encontrada.");
        }

        if (tarefaExistente.UsuarioId != usuarioId)
        {
            throw new InvalidOperationException(
                "Tarefa não pertence ao usuário."
            );
        }

        ValidarDataDeVencimento(tarefa.DataDeVencimento);

        tarefaExistente.Titulo = tarefa.Titulo;
        tarefaExistente.Descricao = tarefa.Descricao;
        tarefaExistente.DataDeVencimento = tarefa.DataDeVencimento;

        await _tarefaRepository.AtualizarAsync(tarefaExistente);

        return tarefaExistente;
    }

    public async Task ConcluirAsync(Guid usuarioId, Guid id)
    {
        var tarefa = await _tarefaRepository.ObterPorIdAsync(id);

        if (tarefa is null)
        {
            throw new InvalidOperationException("Tarefa não encontrada.");
        }

        if (tarefa.UsuarioId != usuarioId)
        {
            throw new InvalidOperationException(
                "Tarefa não pertence ao usuário."
            );
        }

        tarefa.Status = StatusTarefa.Concluida;

        await _tarefaRepository.AtualizarAsync(tarefa);
    }

    public async Task RemoverAsync(Guid usuarioId, Guid id)
    {
        var tarefa = await _tarefaRepository.ObterPorIdAsync(id);

        if (tarefa is null)
        {
            throw new InvalidOperationException("Tarefa não encontrada.");
        }

        if (tarefa.UsuarioId != usuarioId)
        {
            throw new InvalidOperationException(
                "Tarefa não pertence ao usuário."
            );
        }

        await _tarefaRepository.RemoverAsync(tarefa);
    }

    private static void ValidarDataDeVencimento(DateTime dataDeVencimento)
    {
        if (dataDeVencimento <= DateTime.Now)
        {
            throw new InvalidOperationException(
                "A data de vencimento deve ser posterior à data atual."
            );
        }
    }
}