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
            Batteries.Init();
        }

        // Initialize Database (Create Tables if not Exist)
        public static void InitializeDatabase()
        {
            CreateDatabase();
            CreateoutFlowDatabase();
            CreateDebtTrackingDatabase();
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
                        Note TEXT,
                        Tags TEXT
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
                        Note TEXT,
                        Tags TEXT
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

        public static void CreateDebtTrackingDatabase()
        {
            try
            {

                // Open connection to the database
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Create the new DebtTracking table only if it doesn't exist
                    var createTableCmd = @"
                CREATE TABLE IF NOT EXISTS DebtTracking (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    Name TEXT, 
                    Amount REAL, 
                    ClearedAmount REAL,
                    DueDate TEXT
                )";

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = createTableCmd;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                // Log SQLite error
                WriteDebtTrackingError_log($"SQLite Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general error
                WriteDebtTrackingError_log($"Error creating database or table: {ex.Message}");
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

                    var insertCmd = "INSERT INTO CashInflow (Title, Amount, Date, Note, Tags) VALUES (@Title, @Amount, @Date, @Note, @Tags)";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = insertCmd;

                    cmd.Parameters.AddWithValue("@Title", inflow.Title);
                    cmd.Parameters.AddWithValue("@Amount", inflow.Amount);
                    cmd.Parameters.AddWithValue("@Date", inflow.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Note", inflow.Note ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tags", inflow.Tags);

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
                    var insertCmd = "INSERT INTO CashOutflow (Title, Amount, Date, Note, Tags) VALUES (@Title, @Amount, @Date, @Note, @Tags)";
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = insertCmd;

                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@Title", outflow.Title);
                        cmd.Parameters.AddWithValue("@Amount", outflow.Amount);
                        cmd.Parameters.AddWithValue("@Date", outflow.Date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@Note", outflow.Note ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Tags", outflow.Tags);

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

        public static async Task InsertDebtTracking(DebtTrackings debt)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    var insertCmd = "INSERT INTO DebtTracking (Name, Amount, ClearedAmount, DueDate) VALUES (@Name, @Amount, @ClearedAmount, @DueDate)";
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = insertCmd;

                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@Name", debt.Name);
                        cmd.Parameters.AddWithValue("@Amount", debt.Amount);
                        cmd.Parameters.AddWithValue("@ClearedAmount", debt.ClearedAmount);
                        cmd.Parameters.AddWithValue("@DueDate", debt.DueDate.ToString("yyyy-MM-dd"));

                        // Execute the command
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqliteException ex)
            {
                // Log the SQLite error
                WriteDebtTrackingError_log($"SQLite Error while inserting debt tracking record: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log the general error
                WriteDebtTrackingError_log($"Error inserting debt tracking record: {ex.Message}");
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
                    cmd.CommandText = "SELECT Id, Title, Amount, Date, Note, Tags FROM CashInflow";

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
                                Note = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Tags = reader.GetString(5)
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
                    cmd.CommandText = "SELECT Id, Title, Amount, Date, Note, Tags FROM CashOutflow";

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
                                Note = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Tags = reader.GetString(5)
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

        public static async Task<List<DebtTrackings>> GetDebtTrackingAsync()
        {
    var debtTrackings = new List<DebtTrackings>();

    try
    {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Amount, ClearedAmount, DueDate FROM DebtTracking";

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"Raw Amount: {reader.GetValue(2)}");
                    Console.WriteLine($"Raw ClearedAmount: {reader.GetValue(3)}");

                    var debtTracking = new DebtTrackings
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Amount = reader.GetDecimal(2),
                        ClearedAmount = reader.GetDecimal(3),
                        DueDate = DateTime.Parse(reader.GetString(4))
                    };

                    debtTrackings.Add(debtTracking);
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Log the error
        WriteDebtTrackingError_log($"Error fetching debt tracking records: {ex.Message}");

        // Re-throw the exception to let the caller handle it
        throw;
    }

            return debtTrackings;
            //return debtTrackingList;
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

        private static void WriteDebtTrackingError_log(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "log_debttracking_error.txt");

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

        public static async Task<bool> ClearDebtAsync(decimal amount)
        {
            bool success = false;
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            var cmdDebt = connection.CreateCommand();
                            cmdDebt.CommandText = "UPDATE DebtTracking SET Amount = Amount - @amount WHERE Amount >= @amount";
                            cmdDebt.Parameters.AddWithValue("@amount", amount);

                            int debtRowsAffected = await cmdDebt.ExecuteNonQueryAsync();

                            if (debtRowsAffected > 0)
                            {
                                await transaction.CommitAsync();
                                success = true;
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                            }
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteDebtTrackingError_log($"Error clearing debt: {ex.Message}");
            }
            return success;
        }

        public static async Task<decimal> GetTotalDebtLeftAsync()
        {
            decimal totalDebtLeft = 0;
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT SUM(Amount) FROM DebtTracking";
                    var result = await cmd.ExecuteScalarAsync();
                    totalDebtLeft = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                WriteDebtTrackingError_log($"Error calculating total debt left: {ex.Message}");
            }
            return totalDebtLeft;
        }

        // Get the total amount of debt that has been paid off
        public static async Task<decimal> GetTotalDebtPaidAsync(decimal totalDebt)
        {
            decimal totalDebtLeft = await GetTotalDebtLeftAsync();
            decimal totalDebtPaid = totalDebt - totalDebtLeft;
            return totalDebtPaid;
        }

        public static async Task<bool> UpdateDebtTrackingClearedAmountAsync(decimal clearedAmount)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Update the ClearedAmount column in the DebtTracking table
                    string query = "UPDATE DebtTracking SET ClearedAmount = ClearedAmount + @ClearedAmount WHERE Id = 1";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClearedAmount", clearedAmount);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // Return true if rows were updated
                    }
                }
            }
            catch (Exception ex)
            {
                updateDebtTracking($"Error updating DebtTracking table: {ex}");
                return false; // Return false if an error occurs
            }
        }


        
        public static async Task<decimal> GetTotalClearedAmountAsync()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Query to calculate the total ClearedAmount
                    string query = "SELECT SUM(ClearedAmount) FROM DebtTracking";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        object result = await command.ExecuteScalarAsync();
                        // Return the result or 0 if null
                        return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
                    }
                }
            }
            catch (Exception ex)
            {
                updateDebtTracking($"Error calculating total ClearedAmount: {ex}");
                return 0m; // Return 0 if an error occurs
            }
        }

        public static int CalculateTotalTransactions()
        {
            int inflowCount = 0;
            int outflowCount = 0;
            int debtCount = 0;

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Query for the number of transactions in CashInflow
                    using (var inflowCmd = connection.CreateCommand())
                    {
                        inflowCmd.CommandText = "SELECT COUNT(*) FROM CashInflow";
                        var result = inflowCmd.ExecuteScalar();
                        inflowCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }

                    // Query for the number of transactions in CashOutflow
                    using (var outflowCmd = connection.CreateCommand())
                    {
                        outflowCmd.CommandText = "SELECT COUNT(*) FROM CashOutflow";
                        var result = outflowCmd.ExecuteScalar();
                        outflowCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }

                    // Query for the number of debts with remaining balances
                    using (var debtCmd = connection.CreateCommand())
                    {
                        debtCmd.CommandText = "SELECT COUNT(*) FROM DebtTracking WHERE (Amount - ClearedAmount) > 0";
                        var result = debtCmd.ExecuteScalar();
                        debtCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (SqliteException ex)
            {
                calculate($"SQLite Error calculating total transactions count: {ex.Message}");
            }
            catch (Exception ex)
            {
                calculate($"Error calculating total transactions count: {ex.Message}");
            }

            // Calculate total number of transactions
            int totalTransactions = inflowCount + outflowCount + debtCount;
            calculate($"Total number of transactions calculated: {totalTransactions}");
            return totalTransactions;
        }



        private static void updateDebtTracking(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "Upadte_DebtTracking.txt");

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

        private static void calculate(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "calculate.txt");

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

    }
}
