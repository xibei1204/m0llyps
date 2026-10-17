using MollyPS.Util;
using SqlSugar;

namespace MollyPS.Database;

 
public sealed class DatabaseHelper : IDisposable
{
    private static readonly Logger Logger = new("Database");
    private readonly object _gate = new();
    private readonly Dictionary<(Type Type, int Uid), BaseDatabaseDataHelper> _cache = [];
    private SqlSugarScope? _database;
    private bool _disposed;

    public static DatabaseHelper? Instance { get; private set; }

    public void Initialize()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_database != null) return;

            var path = Path.GetFullPath(Path.Combine(ConfigManager.Config.Path.LogPath, "player.db"));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var database = new SqlSugarScope(new ConnectionConfig
            {
                ConnectionString = $"Data Source={path};",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    SerializeService = new CustomSerializeService()
                }
            });

            try
            {
                var models = typeof(BaseDatabaseDataHelper).Assembly.GetTypes()
                    .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BaseDatabaseDataHelper)))
                    .OrderBy(type => type.FullName, StringComparer.Ordinal)
                    .ToArray();
                database.CodeFirst.InitTables(models);
                _database = database;
                Instance = this;
                Logger.Info("Database initialized");
            }
            catch
            {
                database.Dispose();
                throw;
            }
        }
    }

    public T? GetInstance<T>(int uid) where T : BaseDatabaseDataHelper, new()
    {
        lock (_gate)
        {
            var database = GetDatabase();
            if (_cache.TryGetValue((typeof(T), uid), out var cached)) return (T)cached;

            var data = database.Queryable<T>().Where(item => item.Uid == uid).First();
            if (data != null) _cache[(typeof(T), uid)] = data;
            return data;
        }
    }

    public void InsertOrUpdate<T>(T data) where T : BaseDatabaseDataHelper, new()
    {
        ArgumentNullException.ThrowIfNull(data);
        lock (_gate)
        {
            GetDatabase().Storageable(data).ExecuteCommand();
             
            _cache[(typeof(T), data.Uid)] = data;
        }
    }

    private SqlSugarScope GetDatabase()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _database ?? throw new InvalidOperationException("Database is not initialized.");
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _database?.Dispose();
            _database = null;
            _cache.Clear();
            if (ReferenceEquals(Instance, this)) Instance = null;
        }
    }
}
