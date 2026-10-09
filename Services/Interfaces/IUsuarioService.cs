using GerenciadorDeTarefas.Models;

namespace GerenciadorDeTarefas.Services.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> ObterPorIdAsync(Guid id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario> CadastrarAsync(Usuario usuario);
}