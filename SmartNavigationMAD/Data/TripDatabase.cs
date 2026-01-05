using SQLite;
using SmartNavigationMAD.Models;

namespace SmartNavigationMAD.Data;

public class TripDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public TripDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<TripLog>().Wait();
    }

    public Task<int> SaveTripAsync(TripLog trip)
    {
        return _database.InsertAsync(trip);
    }

    public Task<List<TripLog>> GetTripsAsync()
    {
        return _database.Table<TripLog>()
                        .OrderByDescending(t => t.Id)
                        .ToListAsync();
    }

    // NEW: Delete a trip
    public Task<int> DeleteTripAsync(TripLog trip)
    {
        return _database.DeleteAsync(trip);
    }
}
