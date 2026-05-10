using Dapper;
using BadmintonCadastro.Models;

namespace BadmintonCadastro.Services;

internal static class TorneiosService
{
    public static IEnumerable<Torneio> Listar()
    {
        using var conn = DatabaseService.GetConnection();
        return conn.Query<Torneio>(
            "SELECT id, nome, tipo_ficha, data_inicio, local, responsavel_nome, responsavel_tel FROM torneios ORDER BY nome");
    }

    public static Torneio? ObterPorId(int id)
    {
        using var conn = DatabaseService.GetConnection();
        return conn.QuerySingleOrDefault<Torneio>(
            "SELECT id, nome, tipo_ficha, data_inicio, local, responsavel_nome, responsavel_tel FROM torneios WHERE id = @id",
            new { id });
    }

    public static int Inserir(Torneio torneio)
    {
        ValidarTorneio(torneio);

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            INSERT INTO torneios (nome, tipo_ficha, data_inicio, local, responsavel_nome, responsavel_tel)
            VALUES (@Nome, @TipoFicha, @DataInicio, @Local, @ResponsavelNome, @ResponsavelTel);
            SELECT last_insert_rowid();";
        return conn.ExecuteScalar<int>(sql, torneio);
    }

    public static void Atualizar(Torneio torneio)
    {
        ValidarTorneio(torneio);

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            UPDATE torneios
               SET nome             = @Nome,
                   tipo_ficha       = @TipoFicha,
                   data_inicio      = @DataInicio,
                   local            = @Local,
                   responsavel_nome = @ResponsavelNome,
                   responsavel_tel  = @ResponsavelTel
             WHERE id = @Id";
        conn.Execute(sql, torneio);
    }

    public static void Excluir(int id)
    {
        // inscricoes têm ON DELETE CASCADE — são removidas automaticamente
        using var conn = DatabaseService.GetConnection();
        conn.Execute("DELETE FROM torneios WHERE id = @id", new { id });
    }

    private static void ValidarTorneio(Torneio t)
    {
        if (string.IsNullOrWhiteSpace(t.Nome))
            throw new ArgumentException("Nome do torneio é obrigatório.");
        if (t.TipoFicha is not ("ESTADUAL" or "REGIONAL" or "INTERESCOLAR"))
            throw new ArgumentException("TipoFicha deve ser ESTADUAL, REGIONAL ou INTERESCOLAR.");
    }
}
