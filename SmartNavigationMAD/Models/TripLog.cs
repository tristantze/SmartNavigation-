using SQLite;

namespace SmartNavigationMAD.Models;

public class TripLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(20)]
    public string TripId { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}
