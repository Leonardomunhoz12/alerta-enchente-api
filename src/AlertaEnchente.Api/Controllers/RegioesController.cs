using AlertaEnchente.Api.Dtos;
using AlertaEnchente.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlertaEnchente.Api.Controllers;

/// <summary>Regiões monitoradas, níveis dos córregos e farol de risco.</summary>
[ApiController]
[Route("regioes")]
public class RegioesController : ControllerBase
{
    private readonly IRegiaoService _servico;

    public RegioesController(IRegiaoService servico)
    {
        _servico = servico;
    }

    /// <summary>Lista as regiões com o nível atual e o risco (verde, amarelo ou vermelho).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegiaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegiaoDto>>> Listar(CancellationToken ct)
        => Ok(await _servico.ListarAsync(ct));

    /// <summary>Quantas regiões estão em cada nível de risco.</summary>
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(ResumoRiscoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumoRiscoDto>> Resumo(CancellationToken ct)
        => Ok(await _servico.ResumoAsync(ct));

    /// <summary>Detalhes de uma região.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RegiaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegiaoDto>> Obter(string id, CancellationToken ct)
    {
        var regiao = await _servico.ObterAsync(id, ct);
        return regiao is null ? NotFound() : Ok(regiao);
    }

    /// <summary>Últimas medições do nível do córrego (mais recentes primeiro).</summary>
    [HttpGet("{id}/leituras")]
    [ProducesResponseType(typeof(IReadOnlyList<LeituraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<LeituraDto>>> Leituras(string id, [FromQuery] int limite = 20, CancellationToken ct = default)
    {
        var leituras = await _servico.ListarLeiturasAsync(id, limite, ct);
        return leituras is null ? NotFound() : Ok(leituras);
    }

    /// <summary>Registra uma nova medição e devolve a região com o risco recalculado.</summary>
    [HttpPost("{id}/leituras")]
    [ProducesResponseType(typeof(RegiaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegiaoDto>> RegistrarLeitura(string id, [FromBody] NovaLeituraRequest pedido, CancellationToken ct)
    {
        var regiao = await _servico.RegistrarLeituraAsync(id, pedido.NivelCm, ct);
        if (regiao is null) return NotFound();
        return CreatedAtAction(nameof(Obter), new { id }, regiao);
    }
}
