using Microsoft.Data.Sqlite;
using ORM.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ORM.ServicesV4
{
    class Program
    {
        const string DbPath = "app.db";

        // Defined at class level so static test methods can access it
        static UserService userService;

        static void Main(string[] args)
        {
            // ---------------------------------------------------------
            // Step 0: Prepare Environment
            // ---------------------------------------------------------
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Initializing Test Environment ---");
            Console.ResetColor();

            SetupDatabase();

            userService = new UserService();

            // ---------------------------------------------------------
            // TEST 1: Regression Test (Standard Add)
            // Goal: Make sure we didn't break the basic Add functionality
            // ---------------------------------------------------------
            RunTestCase("Test 1: Regression - Add Standard User", Test1_AddStandardUser);

            // ---------------------------------------------------------
            // TEST 2: Null Values Support (NEW)
            // Goal: Check if the ORM handles C# nulls correctly (converts to DBNull)
            // ---------------------------------------------------------
            RunTestCase("Test 2: Null Value Support", Test2_NullValueSupport);

            // ---------------------------------------------------------
            // TEST 3: Update Method (Your Exercise)
            // Goal: Check if the Update method changes correct fields AND keeps others intact
            // ---------------------------------------------------------
            RunTestCase("Test 3: Update Method (Deep Verification)", Test3_UpdateMethod);

            // ---------------------------------------------------------
            // TEST 4: Delete Method (NEW)
            // Goal: Verify that a record is completely removed from the database
            // ---------------------------------------------------------
            RunTestCase("Test 4: Delete Method", Test4_DeleteMethod);

            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine("Tests Completed. Press any key to exit...");
            Console.ReadKey();
        }

        static void Test1_AddStandardUser()
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
            int newId = GetUserIdByName("Danny");
            if (newId == -1)
                throw new Exception("User 'Danny' was NOT found in the database.");
        }

        static void Test2_NullValueSupport()
        {
            User nullUser = new User
            {
                Name = "MysteryUser",
                Age = 99,
                Email = null,      // <--- Explicit NULL
                Username = null,   // <--- Explicit NULL
                Password = "123",
                Role = "Guest"
            };

            Console.WriteLine($"   Adding user with NULL Email and Username...");

            // If the bug exists, this line will crash with "Value must be set"
            userService.Add(nullUser);

            // Verification
            int id = GetUserIdByName("MysteryUser");
            if (id == -1)
                throw new Exception("User 'MysteryUser' was NOT found. Did the program crash?");

            // Verify the data is actually null in the DB
            User storedUser = GetUserById(id);
            if (storedUser.Email != null)
                throw new Exception($"Error: Email should be null, but got '{storedUser.Email}'");
        }

        static void Test3_UpdateMethod()
        {
            // 1. Get the ID of 'Danny' (created in Test 1)
            int dannyId = GetUserIdByName("Danny");
            if (dannyId == -1) throw new Exception("Setup failed: Could not find 'Danny' to update.");

            // 2. Prepare updated data
            User updatedData = new User
            {
                Name = "Danny Updated",       // SHOULD CHANGE
                Age = 17,                     // SHOULD CHANGE (was 16)
                Email = "new_email@test.com", // SHOULD CHANGE
                Username = "danny_d",         // SHOULD NOT CHANGE (Keep same)
                Password = "123",             // SHOULD NOT CHANGE (Keep same)
                Role = "Student"              // SHOULD NOT CHANGE (Keep same)
            };

            Console.WriteLine($"   Updating User ID {dannyId}...");

            // 3. CALL THE STUDENT'S METHOD
            userService.Update(updatedData, dannyId);

            // 4. DEEP VERIFICATION
            User userFromDb = GetUserById(dannyId);

            if (userFromDb == null)
                throw new Exception("Critical Error: The user seems to have been deleted!");

            // Check Changed Fields
            if (userFromDb.Name != "Danny Updated")
                throw new Exception($"Update Failed: Name should be 'Danny Updated', but found '{userFromDb.Name}'.");

            if (userFromDb.Age != 17)
                throw new Exception($"Update Failed: Age should be 17, but found '{userFromDb.Age}'.");

            if (userFromDb.Email != "new_email@test.com")
                throw new Exception($"Update Failed: Email should be 'new_email@test.com', but found '{userFromDb.Email}'.");

            // Check Integrity (Fields that shouldn't change)
            if (userFromDb.Username != "danny_d")
                throw new Exception($"Corruption Detected: Username changed to '{userFromDb.Username}' (Should stay 'danny_d').");

            if (userFromDb.Role != "Student")
                throw new Exception($"Corruption Detected: Role changed to '{userFromDb.Role}' (Should stay 'Student').");
        }

        static void Test4_DeleteMethod()
        {
            // 1. Setup: Add a temporary user specifically to test deletion
            User tempUser = new User
            {
                Name = "DeleteMe",
                Age = 99,
                Email = "delete@me.com",
                Username = "delete_user",
                Password = "000",
                Role = "Target"
            };

            Console.WriteLine($"   Adding temporary user 'DeleteMe'...");
            userService.Add(tempUser);

            int targetId = GetUserIdByName("DeleteMe");
            if (targetId == -1)
                throw new Exception("Setup failed: Could not create user to delete.");

            // 2. Execute Delete
            Console.WriteLine($"   Deleting User ID {targetId}...");
            userService.Delete(targetId);

            // 3. Verify
            User checkUser = GetUserById(targetId);
            if (checkUser != null)
                throw new Exception("Delete Failed: User was still found in the database.");
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

            using (SqliteConnection connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
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

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Database 'app.db' reset successfully.");
        }

        static int GetUserIdByName(string name)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT Id FROM Users WHERE Name = @name LIMIT 1";
                command.Parameters.AddWithValue("@name", name);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        // Helper to fetch the full object for Deep Verification
        static User GetUserById(int id)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={DbPath}"))
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Name = reader["Name"].ToString(),
                            Age = Convert.ToInt32(reader["Age"]),
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                            Username = reader["Username"] != DBNull.Value ? reader["Username"].ToString() : null,
                            Password = reader["Password"] != DBNull.Value ? reader["Password"].ToString() : null,
                            Role = reader["Role"] != DBNull.Value ? reader["Role"].ToString() : null
                        };
                    }
                }
            }
            return null;
        }
    }
}