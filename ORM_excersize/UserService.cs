using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using ORM.Models;

namespace ORM.ServicesV4;

public class UserService : BaseDBService<User>
{
    protected override string GetTableName() { return "Users"; }

    protected override Dictionary<string, object> MapColumnNamesToValues(User item)
    {
        var columns = new Dictionary<string, object>();
        columns.Add("Name", item.Name);
        columns.Add("Age", item.Age);
        columns.Add("Username", item.Username);
        columns.Add("Password", item.Password);
        columns.Add("Role", item.Role);
        return columns;
    }

    // --- הוסף את המתודה הזו ---
    // פונקציה לאיפוס הטבלה - מוחקת גרסאות ישנות ויוצרת חדשה עם כל העמודות
    public void ResetTable()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 1. מחיקת הטבלה הישנה אם קיימת
        var dropCmd = new SqliteCommand("DROP TABLE IF EXISTS Users", connection);
        dropCmd.ExecuteNonQuery();

        // 2. יצירת הטבלה מחדש עם הסכמה המעודכנת
        string createSql = @"
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT,
                Age INTEGER,
                Username TEXT,
                Password TEXT,
                Role TEXT
            )";

        var createCmd = new SqliteCommand(createSql, connection);
        createCmd.ExecuteNonQuery();
    }

    public List<User> GetAllUsers()
    {
        var users = new List<User>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // בדיקה שהטבלה בכלל קיימת לפני שמנסים לשלוף (למניעת קריסות)
        var checkCmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Users'", connection);
        if (checkCmd.ExecuteScalar() == null) return users; // רשימה ריקה אם הטבלה לא קיימת

        using var command = new SqliteCommand("SELECT * FROM Users", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var u = new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Age = reader.GetInt32(2),
                // שימוש ב-IsDBNull למקרה שיש שדות ריקים
                Username = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Password = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Role = reader.IsDBNull(5) ? "" : reader.GetString(5)
            };
            users.Add(u);
        }
        return users;
    }
}
// =============================================================
// Models & Services
// =============================================================
