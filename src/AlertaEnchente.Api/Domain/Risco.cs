namespace AlertaEnchente.Api.Domain;

/// <summary>Dados necessários para calcular o risco de uma região.</summary>
public record DadosRisco(int NivelAtualCm, int? NivelAnteriorCm, int CotaAlertaCm, int CotaEmergenciaCm);

/// <summary>Contrato para calcular o farol de risco (permite trocar a regra sem mexer no resto).</summary>
public interface IRiscoCalculadora
{
    NivelRisco Calcular(DadosRisco dados);
}

/// <summary>
/// Regra principal: verde abaixo da cota de atenção, amarelo entre atenção e emergência,
/// vermelho a partir da cota de emergência.
/// </summary>
public class CalculadoraPorCotas : IRiscoCalculadora
{
    public NivelRisco Calcular(DadosRisco dados)
    {
        if (dados.NivelAtualCm >= dados.CotaEmergenciaCm) return NivelRisco.Vermelho;
        if (dados.NivelAtualCm >= dados.CotaAlertaCm) return NivelRisco.Amarelo;
        return NivelRisco.Verde;
    }
}

/// <summary>
/// Decorator: usa a regra interna e sobe um nível quando o córrego está subindo rápido
/// (aumento maior ou igual ao limite entre duas medições).
/// </summary>
public class CalculadoraComTendencia : IRiscoCalculadora
{
    private readonly IRiscoCalculadora _interna;
    private readonly int _subidaRapidaCm;

    public CalculadoraComTendencia(IRiscoCalculadora interna, int subidaRapidaCm = 20)
    {
        _interna = interna ?? throw new ArgumentNullException(nameof(interna));
        _subidaRapidaCm = subidaRapidaCm;
    }

    public NivelRisco Calcular(DadosRisco dados)
    {
        var nivel = _interna.Calcular(dados);
        if (dados.NivelAnteriorCm is null) return nivel;

        var subida = dados.NivelAtualCm - dados.NivelAnteriorCm.Value;
        if (subida < _subidaRapidaCm) return nivel;

        return nivel switch
        {
            NivelRisco.Verde => NivelRisco.Amarelo,
            NivelRisco.Amarelo => NivelRisco.Vermelho,
            _ => NivelRisco.Vermelho
        };
    }
}

/// <summary>Texto de orientação ao morador para cada nível de risco.</summary>
public interface IOrientacaoProvider
{
    string Obter(NivelRisco nivel);
}

public class OrientacaoPadrao : IOrientacaoProvider
{
    public string Obter(NivelRisco nivel) => nivel switch
    {
        NivelRisco.Verde => "Sem risco agora. Continue acompanhando em dias de chuva forte.",
        NivelRisco.Amarelo => "Fique atento. Guarde documentos e remédios em lugar alto e evite atravessar ruas alagadas.",
        _ => "Perigo. Saia de áreas baixas, vá para um local seguro e ligue 199 (Defesa Civil) se precisar de ajuda."
    };
}
