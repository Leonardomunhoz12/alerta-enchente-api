using AlertaEnchente.Api.Domain;
using AlertaEnchente.Api.Dtos;
using AlertaEnchente.Api.Services;
using Xunit;

namespace AlertaEnchente.Tests;

public class OcorrenciaServiceTests : IDisposable
{
    private readonly BancoEmMemoria _banco = new();
    private readonly OcorrenciaService _servico;

    public OcorrenciaServiceTests()
    {
        _servico = new OcorrenciaService(_banco.Contexto);
        _banco.Contexto.Regioes.Add(new Regiao("vila-itaim", "Vila Itaim", "Córrego Lajeado", 80, 120));
        _banco.Contexto.Regioes.Add(new Regiao("jardim-helena", "Jardim Helena", "Córrego Itaim", 80, 120));
        _banco.Contexto.SaveChanges();
    }

    public void Dispose() => _banco.Dispose();

    private static NovaOcorrenciaRequest Pedido(string regiao, TipoOcorrencia tipo, string local) =>
        new() { RegiaoId = regiao, Tipo = tipo, Local = local, Descricao = "teste" };

    [Fact]
    public async Task CriarAsync_ComDadosValidos_GravaEDevolveAOcorrencia()
    {
        var criada = await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.RuaAlagada, "Rua das Flores"));

        Assert.NotNull(criada);
        Assert.True(criada!.Id > 0);
        Assert.Equal(TipoOcorrencia.RuaAlagada, criada.Tipo);
        Assert.Single(await _servico.ListarAsync(null, null));
    }

    [Fact]
    public async Task CriarAsync_ComRegiaoInexistente_DevolveNulo()
    {
        var criada = await _servico.CriarAsync(Pedido("nao-existe", TipoOcorrencia.RuaAlagada, "Rua das Flores"));

        Assert.Null(criada);
        Assert.Empty(await _servico.ListarAsync(null, null));
    }

    [Fact]
    public async Task CriarAsync_ComLocalCurto_LancaExcecaoDeDominio()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.Entulho, "ab")));
    }

    [Fact]
    public async Task ListarAsync_FiltraPorRegiaoEPorTipo()
    {
        await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.RuaAlagada, "Rua A"));
        await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.Entulho, "Rua B"));
        await _servico.CriarAsync(Pedido("jardim-helena", TipoOcorrencia.RuaAlagada, "Rua C"));

        var daVila = await _servico.ListarAsync("vila-itaim", null);
        var alagadas = await _servico.ListarAsync(null, TipoOcorrencia.RuaAlagada);
        var daVilaAlagadas = await _servico.ListarAsync("vila-itaim", TipoOcorrencia.RuaAlagada);

        Assert.Equal(2, daVila.Count);
        Assert.Equal(2, alagadas.Count);
        Assert.Single(daVilaAlagadas);
    }

    [Fact]
    public async Task ListarAsync_DevolveAMaisRecentePrimeiro()
    {
        await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.RuaAlagada, "Primeira"));
        await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.RuaAlagada, "Segunda"));

        var lista = await _servico.ListarAsync(null, null);

        Assert.Equal("Segunda", lista[0].Local);
    }

    [Fact]
    public async Task RemoverAsync_ComIdExistente_ApagaEDevolveVerdadeiro()
    {
        var criada = await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.BueiroEntupido, "Rua D"));

        var removida = await _servico.RemoverAsync(criada!.Id);

        Assert.True(removida);
        Assert.Empty(await _servico.ListarAsync(null, null));
    }

    [Fact]
    public async Task RemoverAsync_ComIdInexistente_DevolveFalso()
    {
        Assert.False(await _servico.RemoverAsync(999));
    }

    [Fact]
    public async Task EstatisticasAsync_AgrupaPorTipoDoMaiorParaOMenor()
    {
        await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.RuaAlagada, "Rua A"));
        await _servico.CriarAsync(Pedido("vila-itaim", TipoOcorrencia.RuaAlagada, "Rua B"));
        await _servico.CriarAsync(Pedido("jardim-helena", TipoOcorrencia.Entulho, "Rua C"));

        var estatisticas = await _servico.EstatisticasAsync();

        Assert.Equal(2, estatisticas.Count);
        Assert.Equal(TipoOcorrencia.RuaAlagada, estatisticas[0].Tipo);
        Assert.Equal(2, estatisticas[0].Total);
        Assert.Equal(1, estatisticas[1].Total);
    }
}
