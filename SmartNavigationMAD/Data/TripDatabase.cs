using SQLite;
using SmartNavigationMAD.Models;

namespace SmartNavigationMAD.Data;

public class TripDatabase
{
    private readonly SQLiteAsyncConnection _database;
    private bool _isInitialized;

    public TripDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _database.CreateTableAsync<TripLog>();
        _isInitialized = true;
    }

    public async Task<int> SaveTripAsync(TripLog trip)
    {
        await InitializeAsync();
        return await _database.InsertAsync(trip);
    }

    public async Task<List<TripLog>> GetTripsAsync()
    {
        await InitializeAsync();
        return await _database.Table<TripLog>()
                              .OrderByDescending(t => t.Id)
                              .ToListAsync();
    }

    public async Task<int> DeleteTripAsync(TripLog trip)
    {
        await InitializeAsync();
        return await _database.DeleteAsync(trip);
    }
}
