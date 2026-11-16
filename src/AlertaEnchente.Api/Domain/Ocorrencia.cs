namespace AlertaEnchente.Api.Domain;

/// <summary>Problema registrado por um morador (rua alagada, bueiro entupido, etc.).</summary>
public class Ocorrencia : EntidadeBase
{
    public int Id { get; private set; }
    public string RegiaoId { get; private set; } = string.Empty;
    public TipoOcorrencia Tipo { get; private set; }
    public string Local { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;

    private Ocorrencia() { } // usado pelo Entity Framework

    public Ocorrencia(string regiaoId, TipoOcorrencia tipo, string local, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(regiaoId))
            throw new ArgumentException("A região é obrigatória.", nameof(regiaoId));
        if (string.IsNullOrWhiteSpace(local) || local.Trim().Length < 3)
            throw new ArgumentException("Informe o local com pelo menos 3 caracteres.", nameof(local));

        RegiaoId = regiaoId;
        Tipo = tipo;
        Local = local.Trim();
        Descricao = (descricao ?? string.Empty).Trim();
    }
}
