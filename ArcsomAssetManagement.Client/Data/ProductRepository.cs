using ArcsomAssetManagement.Client.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite;

namespace ArcsomAssetManagement.Client.Data;

/// <summary>
/// Repository class for managing products in the database.
/// </summary>
public class ProductRepository
{
    //TODO: replace code with the _database and linq queries
    private readonly SQLiteAsyncConnection _database;

    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;
    public ProductRepository(ILogger<ProductRepository> logger)
    {
        _database = new SQLiteAsyncConnection(Constants.DatabasePath);
        _logger = logger;
    }

    /// <summary>
    /// Initializes the database connection and creates the Product table if it does not exist.
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
            CREATE TABLE IF NOT EXISTS Product (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                ManufacturerId INTEGER,
                FOREIGN KEY (ManufacturerId) REFERENCES Manufacturer(Id) ON DELETE CASCADE
            );";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating Product table");
            throw;
        }

        _hasBeenInitialized = true;
    }

    /// <summary>
    /// Retrieves a list of all products from the database.
    /// </summary>
    /// <returns>A list of <see cref="Product"/> objects.</returns>
    public async Task<List<Product>> ListAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Product";
        var products = new List<Product>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = Convert.ToUInt64(reader.GetInt64(0)),
                Name = reader.GetString(1)
            });
        }

        //foreach (var product in products)
        //{
        //    product.Tags = await _tagRepository.ListAsync(product.Id);
        //    product.Tasks = await _taskRepository.ListAsync(product.Id);
        //}

        return products;
    }

    /// <summary>
    /// Retrieves a list of all products from the database from a certain manufacturer.
    /// </summary>
    /// <returns>A list of <see cref="Product"/> objects.</returns>
    public async Task<List<Product>> ListAsync(ulong manufacturerId)
    {
        return await _database.Table<Product>()
                              .Where(p => p.Manufacturer.Id == manufacturerId)
                              .ToListAsync();

        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Product";
        var products = new List<Product>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = Convert.ToUInt64(reader.GetInt64(0)),
                Name = reader.GetString(1)
            });
        }

        //foreach (var product in products)
        //{
        //    product.Tags = await _tagRepository.ListAsync(product.Id);
        //    product.Tasks = await _taskRepository.ListAsync(product.Id);
        //}

        return products;
    }

    /// <summary>
    /// Retrieves a specific product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product.</param>
    /// <returns>A <see cref="Product"/> object if found; otherwise, null.</returns>
    public async Task<Product?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Product WHERE Id = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var product = new Product
            {
                Id = Convert.ToUInt64(reader.GetInt64(0)),
                Name = reader.GetString(1)
            };

            //product.Tags = await _tagRepository.ListAsync(product.ID);
            //product.Tasks = await _taskRepository.ListAsync(product.ID);

            return product;
        }

        return null;
    }

    /// <summary>
    /// Saves a product to the database. If the product Id is 0, a new product is created; otherwise, the existing product is updated.
    /// </summary>
    /// <param name="item">The product to save.</param>
    /// <returns>The ID of the saved product.</returns>
    public async Task<ulong> SaveItemAsync(Product item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var saveCmd = connection.CreateCommand();
        if (item.Id == 0)
        {
            saveCmd.CommandText = @"
                INSERT INTO Product (Name, ManufacturerId)
                VALUES (@Name, (SELECT Id FROM Manufacturer WHERE Id = @ManufacturerId));
                SELECT last_insert_rowid();";
        }
        else
        {
            saveCmd.CommandText = @"
                UPDATE Product
                SET Name = @Name,
                    ManufacturerId = (SELECT Id FROM Manufacturer WHERE Id = @ManufacturerId)
                WHERE Id = @Id";
            saveCmd.Parameters.AddWithValue("@Id", item.Id);
        }

        if (item.Manufacturer?.Id == null)
        {
            throw new InvalidOperationException("A valid Manufacturer must be provided.");
        }

        saveCmd.Parameters.AddWithValue("@Name", item.Name);
        saveCmd.Parameters.AddWithValue("@ManufacturerId", item.Manufacturer.Id);


        var result = await saveCmd.ExecuteScalarAsync();
        if (item.Id == 0)
        {
            item.Id = Convert.ToUInt64(result);
        }

        return item.Id;
    }

    /// <summary>
    /// Deletes a product from the database.
    /// </summary>
    /// <param name="item">The product to delete.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> DeleteItemAsync(Product item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Product WHERE Id = @Id";
        deleteCmd.Parameters.AddWithValue("@Id", item.Id);

        return await deleteCmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Drops the Product table from the database.
    /// </summary>
    public async Task DropTableAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var dropCmd = connection.CreateCommand();
        dropCmd.CommandText = "DROP TABLE IF EXISTS Product";
        await dropCmd.ExecuteNonQueryAsync();

        //await _taskRepository.DropTableAsync();
        //await _tagRepository.DropTableAsync();
        _hasBeenInitialized = false;
    }
}
