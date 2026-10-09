using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDeTarefas.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(CadastrarUsuarioDto dto)
    {
        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Senha = dto.Senha
        };

        var usuarioCadastrado = await _usuarioService.CadastrarAsync(usuario);

        var resposta = new UsuarioDto
        {
            Id = usuarioCadastrado.Id,
            Nome = usuarioCadastrado.Nome,
            Email = usuarioCadastrado.Email
        };

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = usuarioCadastrado.Id },
            resposta
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        var resposta = new UsuarioDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email
        };

        return Ok(resposta);
    }
}