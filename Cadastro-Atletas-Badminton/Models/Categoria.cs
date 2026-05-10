namespace BadmintonCadastro.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public double ChavePlanilha { get; set; }
    public string? Descricao { get; set; }
    public string Tipo { get; set; } = string.Empty;   // SIMPLES | DUPLA | MISTA
}
