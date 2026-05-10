using Dapper;
using BadmintonCadastro.Models;

namespace BadmintonCadastro.Services;

internal static class AtletasService
{
    // Listagem simples sem JOIN
    public static IEnumerable<Atleta> Listar()
    {
        using var conn = DatabaseService.GetConnection();
        return conn.Query<Atleta>(
            "SELECT id, entidade_id, nome_completo, ano_nascimento, sexo, codigo_federacao FROM atletas ORDER BY nome_completo");
    }

    // Listagem com Entidade aninhada via JOIN, com filtros opcionais
    public static IEnumerable<Atleta> ListarComEntidade(
        string? nome = null, string? sexo = null, int? entidadeId = null)
    {
        using var conn = DatabaseService.GetConnection();

        const string sql = @"
            SELECT
                a.id, a.entidade_id, a.nome_completo, a.ano_nascimento, a.sexo, a.codigo_federacao,
                e.id, e.sigla, e.nome_completo, e.cidade
            FROM atletas a
            INNER JOIN entidades e ON e.id = a.entidade_id
            WHERE (@nome       IS NULL OR a.nome_completo LIKE '%' || @nome || '%')
              AND (@sexo       IS NULL OR a.sexo = @sexo)
              AND (@entidadeId IS NULL OR a.entidade_id = @entidadeId)
            ORDER BY a.nome_completo";

        return conn.Query<Atleta, Entidade, Atleta>(
            sql,
            (atleta, entidade) => { atleta.Entidade = entidade; return atleta; },
            new { nome, sexo, entidadeId },
            splitOn: "id");
    }

    public static Atleta? ObterPorId(int id)
    {
        using var conn = DatabaseService.GetConnection();
        return conn.QuerySingleOrDefault<Atleta>(
            "SELECT id, entidade_id, nome_completo, ano_nascimento, sexo, codigo_federacao FROM atletas WHERE id = @id",
            new { id });
    }

    public static int Inserir(Atleta atleta)
    {
        ValidarAtleta(atleta);

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            INSERT INTO atletas (entidade_id, nome_completo, ano_nascimento, sexo, codigo_federacao)
            VALUES (@EntidadeId, @NomeCompleto, @AnoNascimento, @Sexo, @CodigoFederacao);
            SELECT last_insert_rowid();";
        return conn.ExecuteScalar<int>(sql, atleta);
    }

    public static void Atualizar(Atleta atleta)
    {
        ValidarAtleta(atleta);

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            UPDATE atletas
               SET entidade_id      = @EntidadeId,
                   nome_completo    = @NomeCompleto,
                   ano_nascimento   = @AnoNascimento,
                   sexo             = @Sexo,
                   codigo_federacao = @CodigoFederacao
             WHERE id = @Id";
        conn.Execute(sql, atleta);
    }

    public static void Excluir(int id)
    {
        using var conn = DatabaseService.GetConnection();
        conn.Execute("DELETE FROM atletas WHERE id = @id", new { id });
    }

    private static void ValidarAtleta(Atleta a)
    {
        if (string.IsNullOrWhiteSpace(a.NomeCompleto))
            throw new ArgumentException("Nome completo é obrigatório.");
        if (a.EntidadeId <= 0)
            throw new ArgumentException("Entidade é obrigatória.");
        if (a.AnoNascimento < 1900 || a.AnoNascimento > DateTime.Now.Year)
            throw new ArgumentException("Ano de nascimento inválido.");
        if (a.Sexo != "M" && a.Sexo != "F")
            throw new ArgumentException("Sexo deve ser 'M' ou 'F'.");
    }
}
