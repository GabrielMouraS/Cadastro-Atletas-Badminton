using Dapper;
using BadmintonCadastro.Models;

namespace BadmintonCadastro.Services;

internal static class EntidadesService
{
    public static IEnumerable<Entidade> Listar()
    {
        using var conn = DatabaseService.GetConnection();
        return conn.Query<Entidade>(
            "SELECT id, sigla, nome_completo, cidade FROM entidades ORDER BY sigla");
    }

    public static Entidade? ObterPorId(int id)
    {
        using var conn = DatabaseService.GetConnection();
        return conn.QuerySingleOrDefault<Entidade>(
            "SELECT id, sigla, nome_completo, cidade FROM entidades WHERE id = @id",
            new { id });
    }

    public static int Inserir(Entidade entidade)
    {
        if (string.IsNullOrWhiteSpace(entidade.Sigla))
            throw new ArgumentException("Sigla é obrigatória.");
        if (string.IsNullOrWhiteSpace(entidade.NomeCompleto))
            throw new ArgumentException("Nome completo é obrigatório.");

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            INSERT INTO entidades (sigla, nome_completo, cidade)
            VALUES (@Sigla, @NomeCompleto, @Cidade);
            SELECT last_insert_rowid();";
        return conn.ExecuteScalar<int>(sql, entidade);
    }

    public static void Atualizar(Entidade entidade)
    {
        if (string.IsNullOrWhiteSpace(entidade.Sigla))
            throw new ArgumentException("Sigla é obrigatória.");
        if (string.IsNullOrWhiteSpace(entidade.NomeCompleto))
            throw new ArgumentException("Nome completo é obrigatório.");

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            UPDATE entidades
               SET sigla = @Sigla, nome_completo = @NomeCompleto, cidade = @Cidade
             WHERE id = @Id";
        conn.Execute(sql, entidade);
    }

    public static void Excluir(int id)
    {
        using var conn = DatabaseService.GetConnection();

        int vinculados = conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM atletas WHERE entidade_id = @id", new { id });

        if (vinculados > 0)
            throw new InvalidOperationException(
                $"Não é possível excluir a entidade: há {vinculados} atleta(s) vinculado(s).");

        conn.Execute("DELETE FROM entidades WHERE id = @id", new { id });
    }
}
