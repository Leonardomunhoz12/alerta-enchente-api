using AlertaEnchente.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AlertaEnchente.Api.Data;

/// <summary>Carrega dados iniciais de demonstração (as mesmas regiões do app mobile).</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Regioes.AnyAsync(ct)) return;

        var agora = DateTime.UtcNow;
        var dados = new[]
        {
            ("jardim-helena", "Jardim Helena", "Córrego Itaim", 80, 120, 92),
            ("vila-itaim", "Vila Itaim", "Córrego Lajeado", 80, 120, 41),
            ("itaim-paulista", "Itaim Paulista", "Rio Itaquera", 90, 130, 134),
            ("sao-miguel", "São Miguel Paulista", "Córrego Água Vermelha", 85, 125, 58)
        };

        foreach (var (id, nome, corrego, alerta, emergencia, nivel) in dados)
        {
            var regiao = new Regiao(id, nome, corrego, alerta, emergencia);
            regiao.RegistrarLeitura(Math.Max(0, nivel - 4), agora.AddMinutes(-30));
            regiao.RegistrarLeitura(nivel, agora);
            db.Regioes.Add(regiao);
        }

        db.Ocorrencias.Add(new Ocorrencia("jardim-helena", TipoOcorrencia.RuaAlagada, "Rua das Flores, perto da ponte", "A água já chegou na calçada"));
        db.Ocorrencias.Add(new Ocorrencia("itaim-paulista", TipoOcorrencia.CorregoSubindo, "Margem do Rio Itaquera", ""));
        db.Ocorrencias.Add(new Ocorrencia("jardim-helena", TipoOcorrencia.BueiroEntupido, "Avenida Principal, 120", "Bueiro cheio de lixo"));

        await db.SaveChangesAsync(ct);
    }
}
