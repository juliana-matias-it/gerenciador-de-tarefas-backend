using GerenciadorDeTarefas.Models;

namespace GerenciadorDeTarefas.Services.Interfaces;

public interface ITarefaService
{
    Task<IEnumerable<Tarefa>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<Tarefa?> ObterPorIdAsync(Guid id);
    Task<Tarefa> CriarAsync(Guid usuarioId, Tarefa tarefa);
    Task<Tarefa> AtualizarAsync(Guid usuarioId, Guid id, Tarefa tarefa);
    Task ConcluirAsync(Guid usuarioId, Guid id);
    Task RemoverAsync(Guid usuarioId, Guid id);
}