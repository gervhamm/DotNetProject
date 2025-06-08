using SQLite;

namespace ArcsomAssetManagement.Client.Models;

public class SyncQueueItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string EntityType { get; set; }
    public ulong EntityId { get; set; }
    public OperationType OperationType { get; set; }
    public string PayloadJson { get; set; }
}

public enum OperationType
{
    Create, Update, Delete
}
