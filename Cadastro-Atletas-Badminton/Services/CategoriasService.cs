using Dapper;
using BadmintonCadastro.Models;

namespace BadmintonCadastro.Services;

internal static class CategoriasService
{
    public static IEnumerable<Categoria> Listar()
    {
        using var conn = DatabaseService.GetConnection();
        return conn.Query<Categoria>(
            "SELECT id, codigo, chave_planilha, descricao, tipo FROM categorias ORDER BY tipo, codigo");
    }

    public static IEnumerable<Categoria> ListarPorTipo(string tipo)
    {
        using var conn = DatabaseService.GetConnection();
        return conn.Query<Categoria>(
            "SELECT id, codigo, chave_planilha, descricao, tipo FROM categorias WHERE tipo = @tipo ORDER BY codigo",
            new { tipo });
    }

    public static Categoria? ObterPorId(int id)
    {
        using var conn = DatabaseService.GetConnection();
        return conn.QuerySingleOrDefault<Categoria>(
            "SELECT id, codigo, chave_planilha, descricao, tipo FROM categorias WHERE id = @id",
            new { id });
    }

    public static int Inserir(Categoria categoria)
    {
        ValidarCategoria(categoria);

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            INSERT INTO categorias (codigo, chave_planilha, descricao, tipo)
            VALUES (@Codigo, @ChavePlanilha, @Descricao, @Tipo);
            SELECT last_insert_rowid();";
        return conn.ExecuteScalar<int>(sql, categoria);
    }

    public static void Atualizar(Categoria categoria)
    {
        ValidarCategoria(categoria);

        using var conn = DatabaseService.GetConnection();
        const string sql = @"
            UPDATE categorias
               SET codigo         = @Codigo,
                   chave_planilha = @ChavePlanilha,
                   descricao      = @Descricao,
                   tipo           = @Tipo
             WHERE id = @Id";
        conn.Execute(sql, categoria);
    }

    public static void Excluir(int id)
    {
        using var conn = DatabaseService.GetConnection();

        int vinculadas = conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM inscricoes WHERE categoria_id = @id", new { id });

        if (vinculadas > 0)
            throw new InvalidOperationException(
                $"Não é possível excluir a categoria: há {vinculadas} inscrição(ões) vinculada(s).");

        conn.Execute("DELETE FROM categorias WHERE id = @id", new { id });
    }

    private static void ValidarCategoria(Categoria c)
    {
        if (string.IsNullOrWhiteSpace(c.Codigo))
            throw new ArgumentException("Código é obrigatório.");
        if (c.Tipo is not ("SIMPLES" or "DUPLA" or "MISTA"))
            throw new ArgumentException("Tipo deve ser SIMPLES, DUPLA ou MISTA.");
    }
}
