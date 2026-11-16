using System.Text.Json.Serialization;
using AlertaEnchente.Api.Data;
using AlertaEnchente.Api.Domain;
using AlertaEnchente.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Banco SQLite (arquivo alertaenchente.db na pasta do projeto)
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=alertaenchente.db";
builder.Services.AddDbContext<AppDbContext>(opcoes => opcoes.UseSqlite(connectionString));

// Injeção de dependência
builder.Services.AddScoped<IRegiaoService, RegiaoService>();
builder.Services.AddScoped<IOcorrenciaService, OcorrenciaService>();
builder.Services.AddSingleton<IOrientacaoProvider, OrientacaoPadrao>();
builder.Services.AddSingleton<IRiscoCalculadora>(_ =>
{
    IRiscoCalculadora regra = new CalculadoraPorCotas();
    if (builder.Configuration.GetValue<bool>("Risco:UsarTendencia"))
        regra = new CalculadoraComTendencia(regra);
    return regra;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Libera o app mobile (rodando no navegador) a chamar a API
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Cria o banco e carrega os dados de demonstração
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

// Erros inesperados viram uma resposta JSON padronizada
app.UseExceptionHandler(erroApp => erroApp.Run(async contexto =>
{
    var erro = contexto.Features.Get<IExceptionHandlerFeature>()?.Error;
    var status = erro is ArgumentException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
    contexto.Response.StatusCode = status;
    await contexto.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = status,
        Title = status == StatusCodes.Status400BadRequest ? "Requisição inválida" : "Erro interno",
        Detail = status == StatusCodes.Status400BadRequest ? erro?.Message : "Ocorreu um erro inesperado."
    });
}));

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
