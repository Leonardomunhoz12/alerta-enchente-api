using AlertaEnchente.Api.Dtos;

namespace AlertaEnchente.Api.Services;

public interface IRegiaoService
{
    Task<IReadOnlyList<RegiaoDto>> ListarAsync(CancellationToken ct = default);
    Task<RegiaoDto?> ObterAsync(string id, CancellationToken ct = default);

    /// <summary>Devolve null quando a região não existe.</summary>
    Task<IReadOnlyList<LeituraDto>?> ListarLeiturasAsync(string id, int limite, CancellationToken ct = default);

    /// <summary>Devolve null quando a região não existe.</summary>
    Task<RegiaoDto?> RegistrarLeituraAsync(string id, int nivelCm, CancellationToken ct = default);

    Task<ResumoRiscoDto> ResumoAsync(CancellationToken ct = default);
}
