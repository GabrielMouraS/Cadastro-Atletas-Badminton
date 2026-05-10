namespace BadmintonCadastro.Models;

public class Entidade
{
    public int Id { get; set; }
    public string Sigla { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Cidade { get; set; }
}
