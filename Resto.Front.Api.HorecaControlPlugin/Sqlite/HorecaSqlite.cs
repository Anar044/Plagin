using LinqToDB;
using LinqToDB.Data;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;
using System.IO;
using System.Linq;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite;

public partial class HorecaSqlite : DataConnection, IDisposable
{
    private static string dbPath = Path.Combine(PluginHelpers.StorageDirectory, $"horeca_{PluginHelpers.VersionDB}.db");
    private static string connectionString = $"Data Source={dbPath};Version=3;";

    private bool firstTimeStart;
    private bool _isInitialized = false;
    private readonly object _initLock = new object();

    public HorecaSqlite() : base(new DataOptions().UseSQLite(connectionString))
    {
        try
        {
            EnsureDatabaseCreated();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Log.Error($"HorecaSqlite :: Failed to initialize database: {ex.Message}", ex);
            Log.Error($"HorecaSqlite :: Exception type: {ex.GetType().FullName}");
            if (ex.InnerException != null)
            {
                Log.Error($"HorecaSqlite :: Inner exception: {ex.InnerException.GetType().FullName} - {ex.InnerException.Message}");
            }
            Log.Error($"HorecaSqlite :: Database path: {dbPath}");
            Log.Error($"HorecaSqlite :: Connection string: {connectionString}");
            throw; // Пробрасываем исключение дальше, чтобы DI контейнер знал об ошибке
        }
    }

    public ITable<User> Users => this.GetTable<User>();
    public ITable<Event> Events => this.GetTable<Event>();
    public ITable<Message> Messages => this.GetTable<Message>();
    public ITable<Shift> Shifts => this.GetTable<Shift>();
    public ITable<Order> Orders => this.GetTable<Order>();
    public ITable<HighRiskOperation> HighRiskOperations => this.GetTable<HighRiskOperation>();


    public void EnsureDatabaseCreated()
    {
        lock (_initLock)
        {
            if (_isInitialized)
                return;

            try
            {
                Log.Info($"HorecaSqlite :: Initializing database at: {dbPath}");

                if (!TableExists<User>())
                {
                    this.CreateTable<User>();
                    firstTimeStart = true;
                    Log.Info("HorecaSqlite :: Created Users table");
                }

                if (!TableExists<Event>())
                {
                    this.CreateTable<Event>();
                    firstTimeStart = true;
                    Log.Info("HorecaSqlite :: Created Events table");
                }

                if (!TableExists<Message>())
                {
                    this.CreateTable<Message>();
                    firstTimeStart = true;
                    Log.Info("HorecaSqlite :: Created Messages table");
                }

                if (!TableExists<Shift>())
                {
                    this.CreateTable<Shift>();
                    firstTimeStart = true;
                    Log.Info("HorecaSqlite :: Created Shifts table");
                }

                if (!TableExists<Order>())
                {
                    this.CreateTable<Order>();
                    firstTimeStart = true;
                    Log.Info("HorecaSqlite :: Created Orders table");
                }

                if (!TableExists<HighRiskOperation>())
                {
                    this.CreateTable<HighRiskOperation>();
                    firstTimeStart = true;
                    Log.Info("HorecaSqlite :: Created HighRiskOperations table");
                }

                this.Execute("CREATE INDEX IF NOT EXISTS idx_Users_UserId ON Users(UserId);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_HighRiskOperations_UserId ON HighRiskOperations(UserId);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_HighRiskOperations_ShiftId ON HighRiskOperations(ShiftId);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_Orders_ShiftId ON Orders(ShiftId);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_Orders_OrderId ON Orders(OrderId);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_Shifts_OpenerUserId ON Shifts(OpenerUserId);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_Shifts_CloserUserId ON Shifts(CloserUserId);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_Events_Uuid ON Events(Uuid);");
                this.Execute("CREATE INDEX IF NOT EXISTS idx_Messages_Uuid ON Messages(Uuid);");

                Log.Info("HorecaSqlite :: Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"HorecaSqlite :: Error in EnsureDatabaseCreated: {ex.Message}", ex);
                Log.Error($"HorecaSqlite :: Exception type: {ex.GetType().FullName}");
                if (ex.InnerException != null)
                {
                    Log.Error($"HorecaSqlite :: Inner exception: {ex.InnerException.GetType().FullName} - {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Log.Error($"HorecaSqlite :: Inner-Inner exception: {ex.InnerException.InnerException.GetType().FullName} - {ex.InnerException.InnerException.Message}");
                    }
                }
                Log.Error($"HorecaSqlite :: Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }

    private bool TableExists<T>() where T : class
    {
        var tableName = this.GetTable<T>().TableName;
        var query = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";

        var result = this.Query<string>(query).FirstOrDefault();
        return !string.IsNullOrEmpty(result);
    }
}