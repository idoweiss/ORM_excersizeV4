using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using ORM.Models; // אם המודל נמצא בקובץ נפרד
// אם המודל לא בקובץ נפרד, המחלקה User מוגדרת בתחתית הקובץ הזה

namespace ORM.ServicesV4 // שיניתי ל-V4 כדי שיתאים לתרגיל
{
    class Program
    {
        const string DbPath = "app.db";

        static void Main(string[] args)
        {
            // ---------------------------------------------------------
            // Step 0: Prepare Environment
            // ---------------------------------------------------------
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Initializing Test Environment ---");
            Console.ResetColor();

            SetupDatabase();
            UserService userService = new UserService();

            // ---------------------------------------------------------
            // TEST 1: Regression Test (Standard Add)
            // Goal: Make sure we didn't break the basic Add functionality
            // ---------------------------------------------------------
            RunTestCase("Test 1: Regression - Add Standard User", () =>
            {
                User student = new User
                {
                    Name = "Danny",
                    Age = 16,
                    Email = "danny@class.com",
                    Username = "danny_d",
                    Password = "123",
                    Role = "Student"
                };

                Console.WriteLine($"   Adding: {student.Name}...");
                userService.Add(student);

                // Verification
                if (!CheckIfUserExists("Danny"))
                    throw new Exception("User 'Danny' was NOT found in the database.");
            });

            

            // ---------------------------------------------------------
            // TEST 2: Update Method (Your Exercise)
            // Goal: Check if the Update method actually changes the DB
            // ---------------------------------------------------------
            RunTestCase("Test 3: Update Method (Your Exercise)", () =>
            {
                // 1. Get the ID of 'Danny' (created in Test 1)
                int dannyId = GetUserIdByName("Danny");
                if (dannyId == -1) throw new Exception("Setup failed: Could not find 'Danny' to update.");

                // 2. Prepare updated data
                User updatedData = new User
                {
                    Name = "Danny Updated",
                    Age = 17, // Changed from 16
                    Email = "new_email@test.com",
                    Username = "danny_d", // Keep same username
                    Password = "123",
                    Role = "Student"
                };

                Console.WriteLine($"   Updating User ID {dannyId} to 'Danny Updated'...");

                // 3. CALL THE STUDENT'S METHOD
                userService.Update(updatedData, dannyId);

                // 4. Verify
                if (CheckIfUserExists("Danny"))
                    throw new Exception("Update Failed: The name is still 'Danny' (Old Name).");

                if (!CheckIfUserExists("Danny Updated"))
                    throw new Exception("Update Failed: Could not find 'Danny Updated' in the database.");
            });

            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine("Tests Completed. Press any key to exit...");
            Console.ReadKey();
        }

        // =============================================================
        // Helper: Test Runner (Handles Colors & Exceptions)
        // =============================================================
        static void RunTestCase(string testName, Action testAction)
        {
            Console.WriteLine($"\n{testName}...");
            try
            {
                testAction.Invoke();

                // Success = GREEN
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ PASSED");
                Console.ResetColor();
            }
            catch (NotImplementedException)
            {
                // Not Implemented = YELLOW
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ NOT IMPLEMENTED (Go finish the code!)");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Failure = RED
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ FAILED");
                Console.WriteLine($"   Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        // =============================================================
        // Database Helpers
        // =============================================================

        static void SetupDatabase()
        {
            if (File.Exists(DbPath)) File.Delete(DbPath);

            using (var connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
                // FIX: Added Username, Password, Role to the CREATE statement
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Age INTEGER,
                        Email TEXT,
                        Username TEXT,
                        Password TEXT,
                        Role TEXT
                    );";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Database 'app.db' reset successfully (Full Schema).");
        }

        static bool CheckIfUserExists(string name)
        {
            using (var connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Users WHERE Name = @name";
                command.Parameters.AddWithValue("@name", name);
                long count = (long)command.ExecuteScalar();
                return count > 0;
            }
        }

        static int GetUserIdByName(string name)
        {
            using (var connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id FROM Users WHERE Name = @name LIMIT 1";
                command.Parameters.AddWithValue("@name", name);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }
    }

 

   
}