using System;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace Shawty.Database
{
    public static class DatabaseManager
    {
        private const string connectionUrl = "DataSource=../sqlite/data.db";

        public static void CreateTable(string tableName, string[,] columns)
        {
            using (var conn = new SqliteConnection(connectionUrl))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string cols = string.Join(", ", Enumerable.Range(0, columns.GetLength(0)).Select(i => $"{columns[i, 0]} {columns[i, 1]}"));
                    cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({cols})";
                    Console.WriteLine(cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void Insert(string tableName, string[] vals)
        {
            using (var conn = new SqliteConnection(connectionUrl))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    string values = string.Join(", ", vals.Select(v => $"'{v}'"));
                    cmd.CommandText = $"INSERT INTO {tableName} VALUES ({values})";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void CreateIndex(string tableName, string columnName)
        {
            using (var conn = new SqliteConnection(connectionUrl))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE INDEX IF NOT EXISTS idx_{tableName}_{columnName} ON {tableName}({columnName})";
                    Console.WriteLine(cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string? GetUrl(string encoded)
        {
            using (var conn = new SqliteConnection(connectionUrl))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT url FROM urls WHERE encoded = '{encoded}' LIMIT 1";
                    var result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        public static void DeleteOldUrls(int hours)
        {
            using (var conn = new SqliteConnection(connectionUrl))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM urls WHERE timestamp < datetime('now', '-{hours} hours')";
                    int deleted = cmd.ExecuteNonQuery();
                    if (deleted > 0)
                    {
                        Console.WriteLine($"Deleted {deleted} expired URLs.");
                    }
                }
            }
        }
    }
}