using Dapper;
using BadmintonCadastro.Models;

namespace BadmintonCadastro.Services;

internal static class InscricoesService
{
    // Retorna inscrições com Atleta1, Atleta2 (se houver) e Categoria populados
    public static IEnumerable<Inscricao> ListarPorTorneio(int torneioId)
    {
        using var conn = DatabaseService.GetConnection();

        const string sql = @"
            SELECT
                i.id, i.torneio_id, i.categoria_id, i.atleta1_id, i.atleta2_id,
                i.rk_interno, i.rk_estadual, i.aceita_remanejamento, i.obs_remanejamento,
                i.valor, i.pago,
                a1.id, a1.entidade_id, a1.nome_completo, a1.ano_nascimento, a1.sexo, a1.codigo_federacao,
                a2.id, a2.entidade_id, a2.nome_completo, a2.ano_nascimento, a2.sexo, a2.codigo_federacao,
                c.id, c.codigo, c.chave_planilha, c.descricao, c.tipo
            FROM inscricoes i
            INNER JOIN atletas  a1 ON a1.id = i.atleta1_id
            LEFT  JOIN atletas  a2 ON a2.id = i.atleta2_id
            INNER JOIN categorias c ON c.id  = i.categoria_id
            WHERE i.torneio_id = @torneioId
            ORDER BY c.codigo, a1.nome_completo";

        return conn.Query<Inscricao, Atleta, Atleta, Categoria, Inscricao>(
            sql,
            (insc, a1, a2, cat) =>
            {
                insc.Atleta1   = a1;
                insc.Atleta2   = a2.Id > 0 ? a2 : null;
                insc.Categoria = cat;
                return insc;
            },
            new { torneioId },
            splitOn: "id,id,id");
    }

    public static Inscricao? ObterPorId(int id)
    {
        using var conn = DatabaseService.GetConnection();
        return conn.QuerySingleOrDefault<Inscricao>(
            @"SELECT id, torneio_id, categoria_id, atleta1_id, atleta2_id,
                     rk_interno, rk_estadual, aceita_remanejamento, obs_remanejamento,
                     valor, pago
              FROM inscricoes WHERE id = @id",
            new { id });
    }

    public static int Inserir(Inscricao inscricao)
    {
        ValidarInscricao(inscricao);

        using var conn = DatabaseService.GetConnection();

        // RF08: atleta1 não pode estar na mesma categoria/torneio
        if (AtletaJaInscrito(conn, inscricao.TorneioId, inscricao.CategoriaId, inscricao.Atleta1Id))
            throw new InvalidOperationException(
                $"RF08: o atleta {inscricao.Atleta1Id} já está inscrito nesta categoria/torneio.");

        // RF08: atleta2 (parceiro) também não pode estar inscrito
        if (inscricao.Atleta2Id.HasValue &&
            AtletaJaInscrito(conn, inscricao.TorneioId, inscricao.CategoriaId, inscricao.Atleta2Id.Value))
            throw new InvalidOperationException(
                $"RF08: o atleta {inscricao.Atleta2Id} já está inscrito nesta categoria/torneio.");

        const string sql = @"
            INSERT INTO inscricoes
                (torneio_id, categoria_id, atleta1_id, atleta2_id,
                 rk_interno, rk_estadual, aceita_remanejamento, obs_remanejamento, valor, pago)
            VALUES
                (@TorneioId, @CategoriaId, @Atleta1Id, @Atleta2Id,
                 @RkInterno, @RkEstadual, @AceitaRemanejamento, @ObsRemanejamento, @Valor, @Pago);
            SELECT last_insert_rowid();";
        return conn.ExecuteScalar<int>(sql, inscricao);
    }

    public static void Atualizar(Inscricao inscricao)
    {
        ValidarInscricao(inscricao);

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            UPDATE inscricoes
               SET torneio_id           = @TorneioId,
                   categoria_id         = @CategoriaId,
                   atleta1_id           = @Atleta1Id,
                   atleta2_id           = @Atleta2Id,
                   rk_interno           = @RkInterno,
                   rk_estadual          = @RkEstadual,
                   aceita_remanejamento = @AceitaRemanejamento,
                   obs_remanejamento    = @ObsRemanejamento,
                   valor                = @Valor,
                   pago                 = @Pago
             WHERE id = @Id";
        conn.Execute(sql, inscricao);
    }

    public static void Excluir(int id)
    {
        using var conn = DatabaseService.GetConnection();
        conn.Execute("DELETE FROM inscricoes WHERE id = @id", new { id });
    }

    private static bool AtletaJaInscrito(
        Microsoft.Data.Sqlite.SqliteConnection conn, int torneioId, int categoriaId, int atletaId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM inscricoes
            WHERE torneio_id  = @torneioId
              AND categoria_id = @categoriaId
              AND (atleta1_id = @atletaId OR atleta2_id = @atletaId)";
        return conn.ExecuteScalar<int>(sql, new { torneioId, categoriaId, atletaId }) > 0;
    }

    private static void ValidarInscricao(Inscricao i)
    {
        if (i.TorneioId  <= 0) throw new ArgumentException("TorneioId é obrigatório.");
        if (i.CategoriaId <= 0) throw new ArgumentException("CategoriaId é obrigatório.");
        if (i.Atleta1Id  <= 0) throw new ArgumentException("Atleta1 é obrigatório.");
        if (i.Atleta2Id.HasValue && i.Atleta2Id == i.Atleta1Id)
            throw new ArgumentException("Atleta1 e Atleta2 não podem ser o mesmo atleta.");
    }
}
