using AlertaEnchente.Api.Domain;
using AlertaEnchente.Api.Dtos;

namespace AlertaEnchente.Api.Services;

public interface IOcorrenciaService
{
    Task<IReadOnlyList<OcorrenciaDto>> ListarAsync(string? regiaoId, TipoOcorrencia? tipo, CancellationToken ct = default);

    /// <summary>Devolve null quando a região informada não existe.</summary>
    Task<OcorrenciaDto?> CriarAsync(NovaOcorrenciaRequest pedido, CancellationToken ct = default);

    /// <summary>Devolve false quando a ocorrência não existe.</summary>
    Task<bool> RemoverAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<EstatisticaOcorrenciaDto>> EstatisticasAsync(CancellationToken ct = default);
}
