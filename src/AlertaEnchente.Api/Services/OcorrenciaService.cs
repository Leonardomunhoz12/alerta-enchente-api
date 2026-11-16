using AlertaEnchente.Api.Data;
using AlertaEnchente.Api.Domain;
using AlertaEnchente.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlertaEnchente.Api.Services;

public class OcorrenciaService : IOcorrenciaService
{
    private readonly AppDbContext _db;

    public OcorrenciaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OcorrenciaDto>> ListarAsync(string? regiaoId, TipoOcorrencia? tipo, CancellationToken ct = default)
    {
        IQueryable<Ocorrencia> consulta = _db.Ocorrencias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(regiaoId))
            consulta = consulta.Where(o => o.RegiaoId == regiaoId);

        if (tipo.HasValue)
            consulta = consulta.Where(o => o.Tipo == tipo.Value);

        var itens = await consulta
            .OrderByDescending(o => o.CriadoEm)
            .ThenByDescending(o => o.Id)
            .ToListAsync(ct);

        return itens.Select(Montar).ToList();
    }

    public async Task<OcorrenciaDto?> CriarAsync(NovaOcorrenciaRequest pedido, CancellationToken ct = default)
    {
        if (!await _db.Regioes.AnyAsync(r => r.Id == pedido.RegiaoId, ct)) return null;

        var ocorrencia = new Ocorrencia(pedido.RegiaoId, pedido.Tipo, pedido.Local, pedido.Descricao);
        _db.Ocorrencias.Add(ocorrencia);
        await _db.SaveChangesAsync(ct);
        return Montar(ocorrencia);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken ct = default)
    {
        var ocorrencia = await _db.Ocorrencias.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (ocorrencia is null) return false;

        _db.Ocorrencias.Remove(ocorrencia);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<EstatisticaOcorrenciaDto>> EstatisticasAsync(CancellationToken ct = default)
    {
        var grupos = await _db.Ocorrencias
            .AsNoTracking()
            .GroupBy(o => o.Tipo)
            .Select(g => new { Tipo = g.Key, Total = g.Count() })
            .ToListAsync(ct);

        return grupos
            .OrderByDescending(g => g.Total)
            .ThenBy(g => g.Tipo)
            .Select(g => new EstatisticaOcorrenciaDto(g.Tipo, g.Total))
            .ToList();
    }

    private static OcorrenciaDto Montar(Ocorrencia o) =>
        new(o.Id, o.RegiaoId, o.Tipo, o.Local, o.Descricao, DateTime.SpecifyKind(o.CriadoEm, DateTimeKind.Utc));
}
