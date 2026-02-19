using Microsoft.Data.Sqlite;
using ORM.Models;
using System;
using System.Collections.Generic;

namespace ORM.ServicesV4;

public class RecommendationService : BaseDBService<Recommendation>
{
    // 1. הגדרת שם הטבלה
    // Defining the table name
    protected override string GetTableName()
    {
        return "Recommendations";
    }

    // 2. מיפוי אובייקט לערכים (עבור Add ו-Update)
    // Mapping object properties to database columns (for Add and Update)
    protected override Dictionary<string, object> MapColumnNamesToValues(Recommendation item)
    {
        var columns = new Dictionary<string, object>();

        columns.Add("Type", item.Type);
        columns.Add("Description", item.Description);
        columns.Add("Date", item.Date); // SQLite handles DateTime automatically as string
        columns.Add("Reporter", item.Reporter);

        return columns;
    }

    // 3. שליפת כל ההמלצות (מיפוי ידני משורות ב-DB לאובייקטים)
    // Fetching all recommendations (Manual mapping from DB rows to objects)
    public List<Recommendation> GetAllRecommendations()
    {
        var list = new List<Recommendation>();

        using var connection = _connectionManager.OpenConnection();

        // הנחה: הטבלה נוצרה בסדר העמודות הבא: Id, Type, Description, Date, Reporter
        // Assumption: Table columns are ordered: Id, Type, Description, Date, Reporter
        string sql = "SELECT * FROM Recommendations";

        using var command = new SqliteCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var r = new Recommendation();

            // מיפוי לפי מיקום העמודה (Index)
            // Mapping by column index
            r.Id = reader.GetInt32(0);
            r.Type = reader.GetString(1);
            r.Description = reader.GetString(2);
            r.Date = reader.GetDateTime(3); // המרה אוטומטית מטקסט לתאריך
            r.Reporter = reader.GetString(4);

            list.Add(r);
        }

        return list;
    }

    // מתודת עזר ליצירת הטבלה (אם היא לא קיימת) - אופציונלי אך שימושי לדוגמה
    // Helper method to create the table (if not exists) - optional but useful for demo
    public void CreateTable()
    {
        using var connection = _connectionManager.OpenConnection();

        string sql = @"
            CREATE TABLE IF NOT EXISTS Recommendations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Type TEXT,
                Description TEXT,
                Date TEXT,
                Reporter TEXT
            )";

        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}