namespace AlertaEnchente.Api.Domain;

/// <summary>Região monitorada (um córrego com cotas de atenção e de emergência).</summary>
public class Regiao : EntidadeBase
{
    public string Id { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string Corrego { get; private set; } = string.Empty;
    public int CotaAlertaCm { get; private set; }
    public int CotaEmergenciaCm { get; private set; }
    public ICollection<LeituraNivel> Leituras { get; private set; } = new List<LeituraNivel>();

    private Regiao() { } // usado pelo Entity Framework

    public Regiao(string id, string nome, string corrego, int cotaAlertaCm, int cotaEmergenciaCm)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("O identificador é obrigatório.", nameof(id));
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(corrego))
            throw new ArgumentException("O córrego é obrigatório.", nameof(corrego));
        if (cotaAlertaCm <= 0)
            throw new ArgumentException("A cota de atenção deve ser maior que zero.", nameof(cotaAlertaCm));
        if (cotaEmergenciaCm <= cotaAlertaCm)
            throw new ArgumentException("A cota de emergência deve ser maior que a de atenção.", nameof(cotaEmergenciaCm));

        Id = id;
        Nome = nome;
        Corrego = corrego;
        CotaAlertaCm = cotaAlertaCm;
        CotaEmergenciaCm = cotaEmergenciaCm;
    }

    /// <summary>Registra uma nova medição do nível do córrego.</summary>
    public LeituraNivel RegistrarLeitura(int nivelCm, DateTime? medidoEm = null)
    {
        var leitura = new LeituraNivel(Id, nivelCm, medidoEm ?? DateTime.UtcNow);
        Leituras.Add(leitura);
        return leitura;
    }
}
