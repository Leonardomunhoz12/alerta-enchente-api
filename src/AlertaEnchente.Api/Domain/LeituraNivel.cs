namespace AlertaEnchente.Api.Domain;

/// <summary>Uma medição do nível do córrego, em centímetros.</summary>
public class LeituraNivel : EntidadeBase
{
    public int Id { get; private set; }
    public string RegiaoId { get; private set; } = string.Empty;
    public int NivelCm { get; private set; }
    public DateTime MedidoEm { get; private set; }

    private LeituraNivel() { } // usado pelo Entity Framework

    public LeituraNivel(string regiaoId, int nivelCm, DateTime medidoEm)
    {
        if (string.IsNullOrWhiteSpace(regiaoId))
            throw new ArgumentException("A região é obrigatória.", nameof(regiaoId));
        if (nivelCm < 0)
            throw new ArgumentOutOfRangeException(nameof(nivelCm), "O nível não pode ser negativo.");

        RegiaoId = regiaoId;
        NivelCm = nivelCm;
        MedidoEm = medidoEm;
    }
}
