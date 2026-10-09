using GerenciadorDeTarefas.Enums;

namespace GerenciadorDeTarefas.DTOs;

public class TarefaDto
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public DateTime DataDeVencimento { get; set; }

    public StatusTarefa Status { get; set; }
}