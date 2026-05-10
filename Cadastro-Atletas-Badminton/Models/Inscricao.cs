namespace BadmintonCadastro.Models;

public class Inscricao
{
    public int Id { get; set; }
    public int TorneioId { get; set; }
    public int CategoriaId { get; set; }
    public int Atleta1Id { get; set; }
    public int? Atleta2Id { get; set; }
    public int? RkInterno { get; set; }
    public int? RkEstadual { get; set; }
    public bool AceitaRemanejamento { get; set; }
    public string? ObsRemanejamento { get; set; }
    public double Valor { get; set; }
    public bool Pago { get; set; }

    // Populados via JOIN em ListarPorTorneio
    public Atleta? Atleta1 { get; set; }
    public Atleta? Atleta2 { get; set; }
    public Categoria? Categoria { get; set; }
}
