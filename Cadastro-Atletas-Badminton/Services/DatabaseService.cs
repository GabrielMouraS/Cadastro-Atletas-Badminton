using System.Reflection;
using Microsoft.Data.Sqlite;

namespace BadmintonCadastro.Services;

internal static class DatabaseService
{
    private static readonly string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BadmintonCadastro",
        "dados.db");

    private static readonly string ConnectionString =
        new SqliteConnectionStringBuilder
        {
            DataSource = DbPath,
            ForeignKeys = true
        }.ToString();

    private static readonly object _lock = new();
    private static bool _initialized;

    public static void Initialize()
    {
        lock (_lock)
        {
            if (_initialized) return;

            var dir = Path.GetDirectoryName(DbPath)!;
            Directory.CreateDirectory(dir);

            bool primeiraExecucao = !File.Exists(DbPath);

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            ExecutarSql(conn, "Data.schema.sql");

            if (primeiraExecucao)
                ExecutarSql(conn, "Data.seed.sql");

            _initialized = true;
        }
    }

    public static SqliteConnection GetConnection()
    {
        if (!_initialized)
            throw new InvalidOperationException(
                "DatabaseService.Initialize() deve ser chamado antes de GetConnection().");

        var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    private static void ExecutarSql(SqliteConnection conn, string resourceSuffix)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"BadmintonCadastro.{resourceSuffix}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' não encontrado. " +
                $"Verifique se o Build Action do arquivo é 'Embedded Resource'. " +
                $"Resources disponíveis: {string.Join(", ", assembly.GetManifestResourceNames())}");

        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
