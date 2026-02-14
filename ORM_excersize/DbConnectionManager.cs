using Microsoft.Data.Sqlite;

namespace ORM.Framework;

public class DbConnectionManager
{
    private readonly string _connectionString;

    // We pass the string here once
    public DbConnectionManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}