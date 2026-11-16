using AlertaEnchente.Api.Data;
using AlertaEnchente.Api.Domain;
using AlertaEnchente.Api.Services;
using Xunit;

namespace AlertaEnchente.Tests;

public class RegiaoServiceTests : IDisposable
{
    private readonly BancoEmMemoria _banco = new();
    private readonly RegiaoService _servico;

    public RegiaoServiceTests()
    {
        _servico = new RegiaoService(_banco.Contexto, new CalculadoraPorCotas(), new OrientacaoPadrao());
    }

    public void Dispose() => _banco.Dispose();

    private async Task SemearAsync()
    {
        var vila = new Regiao("vila-itaim", "Vila Itaim", "Córrego Lajeado", 80, 120);
        vila.RegistrarLeitura(30, DateTime.UtcNow.AddMinutes(-10));
        vila.RegistrarLeitura(41, DateTime.UtcNow);

        var itaim = new Regiao("itaim-paulista", "Itaim Paulista", "Rio Itaquera", 90, 130);
        itaim.RegistrarLeitura(134, DateTime.UtcNow);

        _banco.Contexto.Regioes.AddRange(vila, itaim);
        await _banco.Contexto.SaveChangesAsync();
        _banco.Contexto.ChangeTracker.Clear();
    }

    [Fact]
    public async Task ListarAsync_DevolveRegioesEmOrdemAlfabeticaComRisco()
    {
        await SemearAsync();

        var regioes = await _servico.ListarAsync();

        Assert.Equal(2, regioes.Count);
        Assert.Equal("Itaim Paulista", regioes[0].Nome);
        Assert.Equal(NivelRisco.Vermelho, regioes[0].Nivel);
        Assert.Equal("Vila Itaim", regioes[1].Nome);
        Assert.Equal(NivelRisco.Verde, regioes[1].Nivel);
        Assert.Equal(41, regioes[1].NivelCm);
    }

    [Fact]
    public async Task ObterAsync_ComIdExistente_DevolveARegiao()
    {
        await SemearAsync();

        var regiao = await _servico.ObterAsync("itaim-paulista");

        Assert.NotNull(regiao);
        Assert.Equal(134, regiao!.NivelCm);
        Assert.Contains("199", regiao.Orientacao);
    }

    [Fact]
    public async Task ObterAsync_ComIdInexistente_DevolveNulo()
    {
        await SemearAsync();

        Assert.Null(await _servico.ObterAsync("nao-existe"));
    }

    [Fact]
    public async Task RegistrarLeituraAsync_RecalculaORisco()
    {
        await SemearAsync();

        var regiao = await _servico.RegistrarLeituraAsync("vila-itaim", 130);

        Assert.NotNull(regiao);
        Assert.Equal(130, regiao!.NivelCm);
        Assert.Equal(NivelRisco.Vermelho, regiao.Nivel);
    }

    [Fact]
    public async Task RegistrarLeituraAsync_ComRegiaoInexistente_DevolveNulo()
    {
        Assert.Null(await _servico.RegistrarLeituraAsync("nao-existe", 50));
    }

    [Fact]
    public async Task ListarLeiturasAsync_RespeitaOLimiteEAOrdem()
    {
        await SemearAsync();

        var leituras = await _servico.ListarLeiturasAsync("vila-itaim", 1);

        Assert.NotNull(leituras);
        Assert.Single(leituras!);
        Assert.Equal(41, leituras[0].NivelCm);
    }

    [Fact]
    public async Task ListarLeiturasAsync_ComRegiaoInexistente_DevolveNulo()
    {
        Assert.Null(await _servico.ListarLeiturasAsync("nao-existe", 10));
    }

    [Fact]
    public async Task ResumoAsync_ContaRegioesPorNivel()
    {
        await SemearAsync();

        var resumo = await _servico.ResumoAsync();

        Assert.Equal(1, resumo.Verde);
        Assert.Equal(0, resumo.Amarelo);
        Assert.Equal(1, resumo.Vermelho);
        Assert.Equal(2, resumo.TotalRegioes);
    }

    [Fact]
    public async Task SeedAsync_CarregaAsQuatroRegioesUmaUnicaVez()
    {
        await DbSeeder.SeedAsync(_banco.Contexto);
        await DbSeeder.SeedAsync(_banco.Contexto);

        var regioes = await _servico.ListarAsync();

        Assert.Equal(4, regioes.Count);
    }
}
