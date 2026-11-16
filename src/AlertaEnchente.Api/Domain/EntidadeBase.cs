namespace AlertaEnchente.Api.Domain;

/// <summary>Classe base das entidades: guarda a data de criação (sempre em UTC).</summary>
public abstract class EntidadeBase
{
    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;
}
