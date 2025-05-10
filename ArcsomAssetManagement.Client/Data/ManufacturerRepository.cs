using ArcsomAssetManagement.Client.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ArcsomAssetManagement.Client.Data;

/// <summary>
/// Repository class for managing manufacturers in the database.
/// </summary>
public class ManufacturerRepository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;
    public ManufacturerRepository(ILogger<ManufacturerRepository> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes the database connection and creates the Manufacturer table if it does not exist.
    /// </summary>
    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        try
        {
            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Manufacturer (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Contact TEXT NOT NULL
            );";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating Manufacturer table");
            throw;
        }

        _hasBeenInitialized = true;
    }

    /// <summary>
    /// Retrieves a list of all manufacturers from the database.
    /// </summary>
    /// <returns>A list of <see cref="Manufacturer"/> objects.</returns>
    public async Task<List<Manufacturer>> ListAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Manufacturer";
        var manufacturers = new List<Manufacturer>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            manufacturers.Add(new Manufacturer
            {
                Id = Convert.ToUInt64(reader.GetInt64(0)),
                Name = reader.GetString(1),
                Contact = reader.GetString(2)
            });
        }

        //foreach (var manufacturer in manufacturers)
        //{
        //    manufacturer.Tags = await _tagRepository.ListAsync(manufacturer.ID);
        //    manufacturer.Tasks = await _taskRepository.ListAsync(manufacturer.ID);
        //}

        return manufacturers;
    }

    /// <summary>
    /// Retrieves a specific manufacturer by its ID.
    /// </summary>
    /// <param name="id">The ID of the manufacturer.</param>
    /// <returns>A <see cref="Manufacturer"/> object if found; otherwise, null.</returns>
    public async Task<Manufacturer?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Manufacturer WHERE Id = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var manufacturer = new Manufacturer
            {
                Id = Convert.ToUInt64(reader.GetInt32(0)),
                Name = reader.GetString(1),
                Contact = reader.GetString(2)
            };

            //manufacturer.Tags = await _tagRepository.ListAsync(manufacturer.ID);
            //manufacturer.Tasks = await _taskRepository.ListAsync(manufacturer.ID);

            return manufacturer;
        }

        return null;
    }

    /// <summary>
    /// Saves a manufacturer to the database. If the manufacturer Id is 0, a new manufacturer is created; otherwise, the existing manufacturer is updated.
    /// </summary>
    /// <param name="item">The manufacturer to save.</param>
    /// <returns>The ID of the saved manufacturer.</returns>
    public async Task<ulong> SaveItemAsync(Manufacturer item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var saveCmd = connection.CreateCommand();
        if (item.Id == 0)
        {
            saveCmd.CommandText = @"
                INSERT INTO Manufacturer (Name, Contact)
                VALUES (@Name, @Contact);
                SELECT last_insert_rowid();";
        }
        else
        {
            saveCmd.CommandText = @"
                UPDATE Manufacturer
                SET Name = @Name, Contact = @Contact
                WHERE ID = @ID";
            saveCmd.Parameters.AddWithValue("@ID", item.Id);
        }

        saveCmd.Parameters.AddWithValue("@Name", item.Name);
        saveCmd.Parameters.AddWithValue("@Contact", item.Contact);

        var result = await saveCmd.ExecuteScalarAsync();
        if (item.Id == 0)
        {
            item.Id = Convert.ToUInt64(Convert.ToInt64(result));
        }

        return item.Id;
    }

    /// <summary>
    /// Deletes a manufacturer from the database.
    /// </summary>
    /// <param name="item">The manufacturer to delete.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> DeleteItemAsync(Manufacturer item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Manufacturer WHERE ID = @Id";
        deleteCmd.Parameters.AddWithValue("@Id", item.Id);

        return await deleteCmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Drops the Manufacturer table from the database.
    /// </summary>
    public async Task DropTableAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var dropCmd = connection.CreateCommand();
        dropCmd.CommandText = "DROP TABLE IF EXISTS Manufacturer";
        await dropCmd.ExecuteNonQueryAsync();

        //await _taskRepository.DropTableAsync();
        //await _tagRepository.DropTableAsync();
        _hasBeenInitialized = false;
    }
}
