using System;
using System.IO;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace MoneyMap.Data
{
    public static class DatabaseHelper
    {
        private static readonly string dbPath = Path.Combine(FileSystem.AppDataDirectory, "moneyapp.db");

        static DatabaseHelper()
        {
            // Initialize SQLite Batteries
            Batteries.Init();
        }

        // Initialize Database (Create Tables if not Exist)
        public static void InitializeDatabase()
        {
            CreateDatabase();
            CreateoutFlowDatabase();
            try
            {
                WriteErrorInLog($"Database path: {dbPath}");

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    WriteErrorInLog("Database connection opened successfully.");

                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserName TEXT NOT NULL,
                        FullName TEXT NOT NULL,
                        Password TEXT NOT NULL
                    );";

                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        WriteErrorInLog("Users table created or already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteErrorInLog($"Error initializing database: {ex}");
            }
        }

        public static void CreateDatabase()
        {
            try
            {
                // Open connection to the database
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Create the new CashInflow table only if it doesn't exist
                    var createTableCmd = @"
                    CREATE TABLE IF NOT EXISTS CashInflow (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        Title TEXT, 
                        Amount REAL, 
                        Date TEXT,
                        Note TEXT
                )";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = createTableCmd;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                // Log SQLite error
                WriteCashInFlowError_log($"SQLite Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general error
                WriteCashInFlowError_log($"Error creating database or table: {ex.Message}");
            }
        }

        public static void CreateoutFlowDatabase()
        {
            try
            {
                // Open connection to the database
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Create the new CashInflow table only if it doesn't exist
                    var createTableCmd = @"
                    CREATE TABLE IF NOT EXISTS CashOutflow (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        Title TEXT, 
                        Amount REAL, 
                        Date TEXT,
                        Note TEXT
                )";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = createTableCmd;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                // Log SQLite error
                WriteCashOutFlowError_log($"SQLite Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general error
                WriteCashOutFlowError_log($"Error creating database or table: {ex.Message}");
            }
        }



        // Add User
        public static void AddUser(string fullName, string userName, string password)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    string insertQuery = @"
                    INSERT INTO Users (FullName, UserName, Password)
                    VALUES (@FullName, @UserName, @Password);";

                    using (var command = new SqliteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FullName", fullName);
                        command.Parameters.AddWithValue("@UserName", userName);
                        command.Parameters.AddWithValue("@Password", password);
                        command.ExecuteNonQuery();
                    }
                }
                WriteErrorInLog($"User '{userName}' added successfully.");
            }
            catch (SqliteException ex)
            {
                WriteErrorInLog($"SQLite Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                WriteErrorInLog($"Error adding user: {ex}");
            }
        }

        public static async Task InsertCashInflowAsync(CashInFlow inflow)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var insertCmd = "INSERT INTO CashInflow (Title, Amount, Date, Note) VALUES (@Title, @Amount, @Date, @Note)";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = insertCmd;

                    // Add parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@Title", inflow.Title);
                    cmd.Parameters.AddWithValue("@Amount", inflow.Amount);
                    cmd.Parameters.AddWithValue("@Date", inflow.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Note", inflow.Note);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqliteException ex)
            {
                WriteCashInFlowError_log($"SQLite Error while inserting cash inflow: {ex.Message}");
            }
            catch (Exception ex)
            {
                WriteCashInFlowError_log($"Error inserting cash inflow: {ex.Message}");
            }
        }
        public static async Task InsertCashoutFlow(CashOutFlow outflow)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    // SQL command to insert data
                    var insertCmd = "INSERT INTO CashOutflow (Title, Amount, Date, Note) VALUES (@Title, @Amount, @Date, @Note)";
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = insertCmd;

                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@Title", outflow.Title);
                        cmd.Parameters.AddWithValue("@Amount", outflow.Amount);
                        cmd.Parameters.AddWithValue("@Date", outflow.Date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@Note", outflow.Note);

                        // Execute the command
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqliteException ex)
            {
                WriteCashOutFlowError_log($"SQLite Error while inserting cash outflow: {ex.Message}");
            }
            catch (Exception ex)
            {
                WriteCashOutFlowError_log($"Error inserting cash outflow: {ex.Message}");
            }
        }




        // Retrieve All Users
        public static void GetUsers()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    string selectQuery = "SELECT Id, FullName, UserName FROM Users;";
                    using (var command = new SqliteCommand(selectQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WriteErrorInLog($"User: Id={reader.GetInt32(0)}, FullName={reader.GetString(1)}, UserName={reader.GetString(2)}");
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                WriteErrorInLog($"SQLite Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                WriteErrorInLog($"Error retrieving users: {ex}");
            }
        }

        public static async Task<List<CashInFlow>> GetCashInflowsAsync()
        {
            var cashInflows = new List<CashInFlow>();

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT Id, Title, Amount, Date, Note FROM CashInflow";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var cashInFlow = new CashInFlow
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Amount = reader.GetDecimal(2),
                                Date = DateTime.Parse(reader.GetString(3)),
                                Note = reader.GetString(4)
                            };

                            cashInflows.Add(cashInFlow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteCashInFlowError_log($"Error fetching cash inflows: {ex.Message}");
            }

            return cashInflows;
        }

        public static async Task<List<CashOutFlow>> GetCashOutflowsAsync()
        {
            var cashOutflows = new List<CashOutFlow>();

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT Id, Title, Amount, Date, Note FROM CashOutflow";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var cashOutflow = new CashOutFlow
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Amount = reader.GetDecimal(2),
                                Date = DateTime.Parse(reader.GetString(3)),
                                Note = reader.GetString(4)
                            };

                            cashOutflows.Add(cashOutflow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteCashOutFlowError_log($"Error fetching cash outflows: {ex.Message}");
            }

            return cashOutflows;
        }



        // Log Existing Tables
        public static void LogExistingTables()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    string query = "SELECT name FROM sqlite_master WHERE type='table';";
                    using (var command = new SqliteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WriteErrorInLog($"Table: {reader.GetString(0)}");
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                WriteErrorInLog($"SQLite Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                WriteErrorInLog($"Error logging tables: {ex}");
            }
        }

        // Write Log
        private static void WriteErrorInLog(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "log_moneymap_error.txt");

            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write the log message to the file
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Fallback for logging failure
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        private static void WriteCashInFlowError_log(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "log_cashinflow_error.txt");

            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write the log message to the file
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Fallback for logging failure
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }
        private static void WriteCashOutFlowError_log(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "log_cashoutflow_error.txt");

            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write the log message to the file
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Fallback for logging failure
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        public static async Task<decimal> GetTotalInflowAsync()
        {
            decimal total = 0;
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT SUM(Amount) FROM CashInflow";
                    var result = await cmd.ExecuteScalarAsync();
                    total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                WriteCashInFlowError_log($"Error calculating total inflow: {ex.Message}");
            }
            return total;
        }

        public static async Task<decimal> GetTotalOutflowAsync()
        {
            decimal total = 0;
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT SUM(Amount) FROM CashOutflow";
                    var result = await cmd.ExecuteScalarAsync();
                    total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                WriteCashOutFlowError_log($"Error calculating total outflow: {ex.Message}");
            }
            return total;
        }


    }
}
