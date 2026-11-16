using AlertaEnchente.Api.Data;
using AlertaEnchente.Api.Domain;
using AlertaEnchente.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlertaEnchente.Api.Services;

public class RegiaoService : IRegiaoService
{
    private readonly AppDbContext _db;
    private readonly IRiscoCalculadora _calculadora;
    private readonly IOrientacaoProvider _orientacao;

    public RegiaoService(AppDbContext db, IRiscoCalculadora calculadora, IOrientacaoProvider orientacao)
    {
        _db = db;
        _calculadora = calculadora;
        _orientacao = orientacao;
    }

    public async Task<IReadOnlyList<RegiaoDto>> ListarAsync(CancellationToken ct = default)
    {
        var regioes = await _db.Regioes
            .AsNoTracking()
            .Include(r => r.Leituras)
            .OrderBy(r => r.Nome)
            .ToListAsync(ct);

        return regioes.Select(Montar).ToList();
    }

    public async Task<RegiaoDto?> ObterAsync(string id, CancellationToken ct = default)
    {
        var regiao = await _db.Regioes
            .AsNoTracking()
            .Include(r => r.Leituras)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return regiao is null ? null : Montar(regiao);
    }

    public async Task<IReadOnlyList<LeituraDto>?> ListarLeiturasAsync(string id, int limite, CancellationToken ct = default)
    {
        if (!await _db.Regioes.AnyAsync(r => r.Id == id, ct)) return null;

        var leituras = await _db.Leituras
            .AsNoTracking()
            .Where(l => l.RegiaoId == id)
            .OrderByDescending(l => l.MedidoEm)
            .ThenByDescending(l => l.Id)
            .Take(Math.Clamp(limite, 1, 200))
            .ToListAsync(ct);

        return leituras.Select(l => new LeituraDto(l.Id, l.NivelCm, Utc(l.MedidoEm))).ToList();
    }

    public async Task<RegiaoDto?> RegistrarLeituraAsync(string id, int nivelCm, CancellationToken ct = default)
    {
        var regiao = await _db.Regioes
            .Include(r => r.Leituras)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (regiao is null) return null;

        regiao.RegistrarLeitura(nivelCm);
        await _db.SaveChangesAsync(ct);
        return Montar(regiao);
    }

    public async Task<ResumoRiscoDto> ResumoAsync(CancellationToken ct = default)
    {
        var regioes = await ListarAsync(ct);
        var porNivel = regioes
            .GroupBy(r => r.Nivel)
            .ToDictionary(g => g.Key, g => g.Count());

        return new ResumoRiscoDto(
            porNivel.GetValueOrDefault(NivelRisco.Verde),
            porNivel.GetValueOrDefault(NivelRisco.Amarelo),
            porNivel.GetValueOrDefault(NivelRisco.Vermelho),
            regioes.Count);
    }

    private RegiaoDto Montar(Regiao regiao)
    {
        var ultimas = regiao.Leituras
            .OrderByDescending(l => l.MedidoEm)
            .ThenByDescending(l => l.Id)
            .Take(2)
            .ToList();

        var atual = ultimas.FirstOrDefault();
        int? anterior = ultimas.Count > 1 ? ultimas[1].NivelCm : null;
        var nivelCm = atual?.NivelCm ?? 0;

        var nivel = _calculadora.Calcular(new DadosRisco(nivelCm, anterior, regiao.CotaAlertaCm, regiao.CotaEmergenciaCm));

        return new RegiaoDto(
            regiao.Id,
            regiao.Nome,
            regiao.Corrego,
            nivelCm,
            regiao.CotaAlertaCm,
            regiao.CotaEmergenciaCm,
            Utc(atual?.MedidoEm ?? regiao.CriadoEm),
            nivel,
            _orientacao.Obter(nivel));
    }

    // O SQLite devolve datas sem fuso; aqui marcamos como UTC para o JSON sair com "Z".
    private static DateTime Utc(DateTime data) => DateTime.SpecifyKind(data, DateTimeKind.Utc);
}
