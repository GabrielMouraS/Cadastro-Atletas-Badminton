namespace BadmintonCadastro.Models;

public class Atleta
{
    public int Id { get; set; }
    public int EntidadeId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public int AnoNascimento { get; set; }
    public string Sexo { get; set; } = string.Empty;   // "M" ou "F"
    public string? CodigoFederacao { get; set; }

    // Populado via JOIN em ListarComEntidade
    public Entidade? Entidade { get; set; }
}
