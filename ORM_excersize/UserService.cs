using Microsoft.Data.Sqlite;
using ORM.Framework;
using ORM.Models;
using System.Collections.Generic;

namespace ORM.ServicesV4;

public class UserService : BaseDBService<User>
{
    protected override string GetTableName() { return "Users"; }

    protected override Dictionary<string, object> MapColumnNamesToValues(User user)
    {
        var columns = new Dictionary<string, object>();
        columns.Add("Name", user.Name);
        columns.Add("Age", user.Age);
        columns.Add("Username", user.Username);
        columns.Add("Password", user.Password);
        columns.Add("Role", user.Role);
        columns.Add("Email", user.Email);
        return columns;
    }

    // --- הוסף את המתודה הזו ---
    // פונקציה לאיפוס הטבלה - מוחקת גרסאות ישנות ויוצרת חדשה עם כל העמודות
    public void ResetTable()
    {
        using var connection = _connectionManager.OpenConnection();

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
        using var connection = _connectionManager.OpenConnection();

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
