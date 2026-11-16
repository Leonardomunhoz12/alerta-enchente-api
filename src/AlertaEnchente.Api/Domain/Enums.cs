namespace AlertaEnchente.Api.Domain;

/// <summary>Estado do farol de risco de uma região.</summary>
public enum NivelRisco
{
    Verde,
    Amarelo,
    Vermelho
}

/// <summary>Tipos de problema que o morador pode registrar.</summary>
public enum TipoOcorrencia
{
    RuaAlagada,
    CorregoSubindo,
    BueiroEntupido,
    Entulho
}
