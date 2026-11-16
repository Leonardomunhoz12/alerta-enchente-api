using AlertaEnchente.Api.Domain;
using Xunit;

namespace AlertaEnchente.Tests;

public class CalculadoraPorCotasTests
{
    private readonly CalculadoraPorCotas _calculadora = new();

    [Theory]
    [InlineData(0, NivelRisco.Verde)]
    [InlineData(41, NivelRisco.Verde)]
    [InlineData(79, NivelRisco.Verde)]
    [InlineData(80, NivelRisco.Amarelo)]
    [InlineData(92, NivelRisco.Amarelo)]
    [InlineData(119, NivelRisco.Amarelo)]
    [InlineData(120, NivelRisco.Vermelho)]
    [InlineData(134, NivelRisco.Vermelho)]
    public void Calcular_ClassificaPeloNivelEmRelacaoAsCotas(int nivelCm, NivelRisco esperado)
    {
        var resultado = _calculadora.Calcular(new DadosRisco(nivelCm, null, 80, 120));

        Assert.Equal(esperado, resultado);
    }
}

public class CalculadoraComTendenciaTests
{
    private static CalculadoraComTendencia Criar() => new(new CalculadoraPorCotas(), 20);

    [Fact]
    public void Calcular_SemLeituraAnterior_MantemARegraDasCotas()
    {
        var resultado = Criar().Calcular(new DadosRisco(70, null, 80, 120));

        Assert.Equal(NivelRisco.Verde, resultado);
    }

    [Fact]
    public void Calcular_SubidaRapidaNoVerde_SobeParaAmarelo()
    {
        var resultado = Criar().Calcular(new DadosRisco(70, 40, 80, 120));

        Assert.Equal(NivelRisco.Amarelo, resultado);
    }

    [Fact]
    public void Calcular_SubidaRapidaNoAmarelo_SobeParaVermelho()
    {
        var resultado = Criar().Calcular(new DadosRisco(100, 75, 80, 120));

        Assert.Equal(NivelRisco.Vermelho, resultado);
    }

    [Fact]
    public void Calcular_SubidaLenta_NaoMudaONivel()
    {
        var resultado = Criar().Calcular(new DadosRisco(70, 65, 80, 120));

        Assert.Equal(NivelRisco.Verde, resultado);
    }

    [Fact]
    public void Calcular_NivelEmQueda_NaoMudaONivel()
    {
        var resultado = Criar().Calcular(new DadosRisco(100, 130, 80, 120));

        Assert.Equal(NivelRisco.Amarelo, resultado);
    }

    [Fact]
    public void Construtor_ComCalculadoraNula_LancaExcecao()
    {
        Assert.Throws<ArgumentNullException>(() => new CalculadoraComTendencia(null!));
    }
}

public class OrientacaoPadraoTests
{
    [Theory]
    [InlineData(NivelRisco.Verde, "Sem risco")]
    [InlineData(NivelRisco.Amarelo, "Fique atento")]
    [InlineData(NivelRisco.Vermelho, "199")]
    public void Obter_DevolveTextoDoNivel(NivelRisco nivel, string trecho)
    {
        var texto = new OrientacaoPadrao().Obter(nivel);

        Assert.Contains(trecho, texto);
    }
}
