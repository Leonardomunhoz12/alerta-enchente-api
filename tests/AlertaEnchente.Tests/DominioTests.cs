using AlertaEnchente.Api.Domain;
using Xunit;

namespace AlertaEnchente.Tests;

public class RegiaoTests
{
    [Fact]
    public void Construtor_ComDadosValidos_PreencheAsPropriedades()
    {
        var regiao = new Regiao("vila-itaim", "Vila Itaim", "Córrego Lajeado", 80, 120);

        Assert.Equal("vila-itaim", regiao.Id);
        Assert.Equal("Vila Itaim", regiao.Nome);
        Assert.Equal(80, regiao.CotaAlertaCm);
        Assert.Equal(120, regiao.CotaEmergenciaCm);
        Assert.Empty(regiao.Leituras);
    }

    [Fact]
    public void Construtor_ComCotaDeEmergenciaMenorQueAAtencao_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Regiao("x", "X", "Córrego", 100, 100));
    }

    [Fact]
    public void Construtor_SemNome_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Regiao("x", " ", "Córrego", 80, 120));
    }

    [Fact]
    public void RegistrarLeitura_AdicionaALeituraNaRegiao()
    {
        var regiao = new Regiao("x", "X", "Córrego", 80, 120);

        var leitura = regiao.RegistrarLeitura(55);

        Assert.Single(regiao.Leituras);
        Assert.Equal(55, leitura.NivelCm);
        Assert.Equal("x", leitura.RegiaoId);
    }

    [Fact]
    public void RegistrarLeitura_ComNivelNegativo_LancaExcecao()
    {
        var regiao = new Regiao("x", "X", "Córrego", 80, 120);

        Assert.Throws<ArgumentOutOfRangeException>(() => regiao.RegistrarLeitura(-1));
    }
}

public class OcorrenciaTests
{
    [Fact]
    public void Construtor_ComDadosValidos_LimpaOsEspacos()
    {
        var o = new Ocorrencia("vila-itaim", TipoOcorrencia.RuaAlagada, "  Rua das Flores  ", "  água na calçada ");

        Assert.Equal("Rua das Flores", o.Local);
        Assert.Equal("água na calçada", o.Descricao);
        Assert.Equal(TipoOcorrencia.RuaAlagada, o.Tipo);
    }

    [Fact]
    public void Construtor_ComLocalCurto_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Ocorrencia("vila-itaim", TipoOcorrencia.Entulho, "ab", null));
    }

    [Fact]
    public void Construtor_SemDescricao_UsaTextoVazio()
    {
        var o = new Ocorrencia("vila-itaim", TipoOcorrencia.Entulho, "Avenida Principal", null);

        Assert.Equal(string.Empty, o.Descricao);
    }
}
