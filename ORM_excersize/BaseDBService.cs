using Microsoft.Data.Sqlite;
using ORM.Framework;
using System;
using System.Collections.Generic;

namespace ORM.ServicesV4;

public abstract class BaseDBService<T>
{
    // מחרוזת החיבור
    protected DbConnectionManager _connectionManager = new DbConnectionManager("Data Source=app.db");

    protected abstract string GetTableName();
    protected abstract Dictionary<string, object> MapColumnNamesToValues(T item);

    // ---------------------------------------------------------
    // 1. ADD METHOD (Implemented for reference)
    // ---------------------------------------------------------
    public void Add(T item)
    {
        Dictionary<string, object> columnsData = MapColumnNamesToValues(item);

        List<string> columnNames = new List<string>();
        List<string> paramNames = new List<string>();

        foreach (string key in columnsData.Keys)
        {
            columnNames.Add(key);
            paramNames.Add("@" + key);
        }

        string cols = string.Join(", ", columnNames);
        string paramsTxt = string.Join(", ", paramNames);

        string sql = $"INSERT INTO {GetTableName()} ({cols}) VALUES ({paramsTxt})";

        using SqliteConnection connection = _connectionManager.OpenConnection();
        using SqliteCommand command = new SqliteCommand(sql, connection);

        foreach (KeyValuePair<string, object> entry in columnsData)
        {
            // FIX: Convert C# null to DBNull.Value explicitly
            object safeValue = entry.Value ?? DBNull.Value;

            command.Parameters.AddWithValue("@" + entry.Key, safeValue);
        }

        command.ExecuteNonQuery();
    }

    //update table set Name = @Name, Email=@email where Id = @id;
    // ---------------------------------------------------------
    // 2. UPDATE METHOD (EXERCISE)
    // ---------------------------------------------------------
    public void Update(T item, int id)
    {

        throw new NotImplementedException();
    }

    // ---------------------------------------------------------
    // 3. DELETE METHOD (Implemented for reference)
    // ---------------------------------------------------------
    public void Delete(int id)
    {
        string sql = $"DELETE FROM {GetTableName()} WHERE Id = @id";

        using SqliteConnection connection = _connectionManager.OpenConnection();
        using SqliteCommand command = new SqliteCommand(sql, connection);

        command.Parameters.AddWithValue("@id", id);

        command.ExecuteNonQuery();
    }
}