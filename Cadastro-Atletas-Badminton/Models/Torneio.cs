namespace BadmintonCadastro.Models;

public class Torneio
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoFicha { get; set; } = string.Empty;  // ESTADUAL | REGIONAL | INTERESCOLAR
    public DateTime? DataInicio { get; set; }
    public string? Local { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? ResponsavelTel { get; set; }
}
