using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ORM.ServicesV4;

public abstract class BaseDBService<T>
{
    // מחרוזת החיבור
    protected string _connectionString = "Data Source=app.db";

    protected abstract string GetTableName();
    protected abstract Dictionary<string, object> MapColumnNamesToValues(T item);

    // ---------------------------------------------------------
    // 1. ADD METHOD (Implemented for reference)
    // ---------------------------------------------------------
    public void Add(T item)
    {
        Dictionary<string, object> columnsData = MapColumnNamesToValues(item);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

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

        using var command = new SqliteCommand(sql, connection);

        foreach (var entry in columnsData)
        {
            command.Parameters.AddWithValue("@" + entry.Key, entry.Value);
        }

        command.ExecuteNonQuery();
    }

    // ---------------------------------------------------------
    // 2. UPDATE METHOD (EXERCISE)
    // ---------------------------------------------------------
    public void Update(T item, int id)
    {
        // TODO:  הבאת המידע על שמות העמודות לשינוי 

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // TODO: שלב א' - הכנת רשימת השינויים (SET Clause)
        // עליכם ליצור רשימה של מחרוזות. כל מחרוזת צריכה להיות בפורמט: "ColumnName = @ColumnName"
        // השתמשו בלולאה על המפתחות של columnsData

        List<string> setClauses = new List<string>();

        // --- כתבו את הלולאה כאן ---



        // TODO: שלב ב' - בניית השאילתה
        // 1. חברו את הרשימה שיצרתם למחרוזת אחת עם פסיקים (string.Join)
        // 2. צרו את משפט ה-SQL המלא: UPDATE TableName SET ... WHERE Id = @Id

        string sql = ""; // --- השלימו את השורה הזו ---


        using var command = new SqliteCommand(sql, connection);

        // TODO: שלב ג' - הזרקת הערכים (Parameters)
        // עליכם לרוץ בלולאה על המילון columnsData ולהוסיף את הערכים לפקודה (כמו ב-Add)

        // --- כתבו את הלולאה כאן ---



        // TODO: שלב ד' - הוספת ה-ID
        // אל תשכחו! ה-ID הוא הפרמטר הכי חשוב (עבור ה-WHERE). הוסיפו אותו בנפרד.



        // הרצת הפקודה
        // command.ExecuteNonQuery(); // (Uncomment this line when ready)

        // זריקת שגיאה זמנית כדי להזכיר לכם לממש את המתודה
        throw new NotImplementedException("Need to implement the Update method!");
    }

    // ---------------------------------------------------------
    // 3. DELETE METHOD (Implemented for reference)
    // ---------------------------------------------------------
    public void Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string sql = $"DELETE FROM {GetTableName()} WHERE Id = @id";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        command.ExecuteNonQuery();
    }
}