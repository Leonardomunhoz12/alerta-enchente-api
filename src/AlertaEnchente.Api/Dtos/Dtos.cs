using System.ComponentModel.DataAnnotations;
using AlertaEnchente.Api.Domain;

namespace AlertaEnchente.Api.Dtos;

/// <summary>Região com o nível atual do córrego e o risco calculado (mesmo formato usado pelo app mobile).</summary>
public record RegiaoDto(
    string Id,
    string Nome,
    string Corrego,
    int NivelCm,
    int CotaAlertaCm,
    int CotaEmergenciaCm,
    DateTime AtualizadoEm,
    NivelRisco Nivel,
    string Orientacao);

public record LeituraDto(int Id, int NivelCm, DateTime MedidoEm);

public record OcorrenciaDto(int Id, string RegiaoId, TipoOcorrencia Tipo, string Local, string Descricao, DateTime CriadoEm);

public record EstatisticaOcorrenciaDto(TipoOcorrencia Tipo, int Total);

public record ResumoRiscoDto(int Verde, int Amarelo, int Vermelho, int TotalRegioes);

public class NovaLeituraRequest
{
    /// <summary>Nível do córrego em centímetros (0 a 1000).</summary>
    [Range(0, 1000)]
    public int NivelCm { get; init; }
}

public class NovaOcorrenciaRequest
{
    [Required]
    public string RegiaoId { get; init; } = string.Empty;

    [Required]
    public TipoOcorrencia Tipo { get; init; }

    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string Local { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; init; }
}
