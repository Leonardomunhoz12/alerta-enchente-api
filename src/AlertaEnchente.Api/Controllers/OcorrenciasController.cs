using AlertaEnchente.Api.Domain;
using AlertaEnchente.Api.Dtos;
using AlertaEnchente.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlertaEnchente.Api.Controllers;

/// <summary>Problemas registrados pelos moradores.</summary>
[ApiController]
[Route("ocorrencias")]
public class OcorrenciasController : ControllerBase
{
    private readonly IOcorrenciaService _servico;

    public OcorrenciasController(IOcorrenciaService servico)
    {
        _servico = servico;
    }

    /// <summary>Lista as ocorrências, com filtros opcionais por região e por tipo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OcorrenciaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OcorrenciaDto>>> Listar([FromQuery] string? regiaoId, [FromQuery] TipoOcorrencia? tipo, CancellationToken ct)
        => Ok(await _servico.ListarAsync(regiaoId, tipo, ct));

    /// <summary>Total de ocorrências por tipo.</summary>
    [HttpGet("estatisticas")]
    [ProducesResponseType(typeof(IReadOnlyList<EstatisticaOcorrenciaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EstatisticaOcorrenciaDto>>> Estatisticas(CancellationToken ct)
        => Ok(await _servico.EstatisticasAsync(ct));

    /// <summary>Registra uma nova ocorrência.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OcorrenciaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OcorrenciaDto>> Criar([FromBody] NovaOcorrenciaRequest pedido, CancellationToken ct)
    {
        var criada = await _servico.CriarAsync(pedido, ct);
        if (criada is null) return NotFound(new ProblemDetails { Status = 404, Title = "Região não encontrada" });
        return Created($"/ocorrencias/{criada.Id}", criada);
    }

    /// <summary>Apaga uma ocorrência.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
        => await _servico.RemoverAsync(id, ct) ? NoContent() : NotFound();
}
