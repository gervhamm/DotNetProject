﻿using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ManufacturerRepository : IOnlineRepository<ManufacturerDto>
{
    private readonly ILogger<ManufacturerRepository> _logger;
    private SQLiteAsyncConnection? _database;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly ConnectivityService _connectivity;

    private readonly string _apiUrl;

    public ManufacturerRepository(ILogger<ManufacturerRepository> logger, SQLiteAsyncConnection database, IHttpClientFactory httpClientFactory, IMapper mapper, ConnectivityService connectivity)
    {
        _logger = logger;
        _database = database;
        _httpClient = httpClientFactory.CreateClient("AuthorizedClient");
        _mapper = mapper;
        _connectivity = connectivity;
        _apiUrl = "api/manufacturer";
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        var tables = _database.GetTableInfoAsync("Manufacturer").Result;
        if (_database is not null && tables.Count() > 0)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Manufacturer>();
        await _database.CreateTableAsync<SyncQueueItem>();

    }

    public async Task<Manufacturer?> GetAsync(ulong id)
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<ManufacturerDto>();
                    var domainItem = _mapper.Map<Manufacturer>(json ?? new ManufacturerDto());
                    return domainItem;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get manufacturer online.");
            }
        }

        var manufacturer = await _database.FindAsync<Manufacturer>(id);
        var products = _database.Table<Product?>()
            .Where(p => p.ManufacturerId == manufacturer.Id).ToListAsync();
        manufacturer.Products = await products;

        return manufacturer;
    }

    public async Task<List<Manufacturer>> ListAsync()
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ManufacturerDto>>();
                    var domainItems = _mapper.Map<List<Manufacturer>>(dtos ?? new List<ManufacturerDto>());
                    return domainItems;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list manufacturers online.");
                return new List<Manufacturer>();
            }
        }

        try
        {
            return await _database.Table<Manufacturer>().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing manufacturers from database");
            return new List<Manufacturer>();
        }
    }

    public async Task<(List<Manufacturer>, PaginationModel)> ListAsync(int pageNumber = 1, int pageSize = 3, string filter = null, bool desc = false)
    {

        var pagination = new PaginationModel
        {
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalItems = pageSize
        };

        var apiUrlPaged = _apiUrl + $"/Paged?pageNumber={pageNumber}&pageSize={pageSize}&filter={filter}&desc={desc}";

        if (_connectivity.IsOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrlPaged);
                if (response.IsSuccessStatusCode)
                {
                    string totalManufactuers = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ManufacturerDto>>();

                    var domainItems = _mapper.Map<List<Manufacturer>>(dtos ?? new List<ManufacturerDto>());
                    int.TryParse(totalManufactuers, out int totalCount);
                    pagination.TotalItems = totalCount;
                    return (domainItems, pagination);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list manufacturers online.");
                return (new List<Manufacturer>(), pagination);
            }
        }

        try
        {
            var lowerFilter = filter.ToLowerInvariant();
            var totalItems = await _database.Table<Manufacturer>()
                                .Where(p => p.Name.ToLower().Contains(lowerFilter) ||
                                p.Contact.Contains(lowerFilter))
                                .CountAsync();

            AsyncTableQuery<Manufacturer> query = _database.Table<Manufacturer>();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(p => p.Name.ToLower().Contains(lowerFilter) ||
                                              p.Contact.ToLower().Contains(lowerFilter));
            }
            var manufacturers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            pagination.TotalItems = totalItems;

            return (manufacturers, pagination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing manufacturers from database");
            return (new List<Manufacturer>(), pagination);
        }
    }

    public async Task<ulong> SaveItemAsync(Manufacturer item, bool trackSync)
    {
        ManufacturerDto dto = _mapper.Map<ManufacturerDto>(item);
        if (item.Id == 0)
        {
            if (_connectivity.IsOnline)
            {
                await SaveItemOnlineAsync(dto);
                await _database.InsertAsync(item);
                return item.Id;

            }

            await _database.InsertAsync(item); 

            if (trackSync)
            {
                await _database.InsertAsync(new SyncQueueItem
                {
                    EntityType = nameof(Manufacturer),
                    EntityId = item.Id,
                    OperationType = OperationType.Create,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
            return item.Id;
        }
        else
        {

            if (_connectivity.IsOnline)
            {
                await UpdateItemOnlineAsync(dto);
                var updatedRows = await _database.UpdateAsync(item);
                if (updatedRows == 0) await _database.InsertOrReplaceAsync(item);
                return item.Id;
            }
            var result = await _database.UpdateAsync(item);
            if (result == 0) await _database.InsertOrReplaceAsync(item);

            if (trackSync)
            {
                await _database.InsertAsync(new SyncQueueItem
                {
                    EntityType = nameof(Manufacturer),
                    EntityId = item.Id,
                    OperationType = OperationType.Update,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
            return item.Id;
        }
    }
    public async Task SaveItemOnlineAsync(ManufacturerDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}", dto);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error while saving item: {response.StatusCode}");
        }
    }
    public async Task<int> DeleteItemAsync(Manufacturer item)
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                var isDeletedOnline = await DeleteItemOnlineAsync(item.Id);
                if (isDeletedOnline)
                {
                    await _database.DeleteAsync(item);
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete manufacturer online.");
            }
        }

        await _database.InsertAsync(new SyncQueueItem
        {
            EntityType = nameof(Manufacturer),
            EntityId = item.Id,
            OperationType = OperationType.Delete,
            PayloadJson = JsonSerializer.Serialize(item)
        });

        return await _database.DeleteAsync(item);
    }
    public async Task UpdateItemOnlineAsync(ManufacturerDto dto)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{_apiUrl}/{dto.Id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error while updating item: {response.StatusCode}");
        }
    }
    public async Task<bool> DeleteItemOnlineAsync(ulong id)
    {
        var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
    public async Task<IEnumerable<ManufacturerDto>> ListOnlineAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ManufacturerDto>>();
                return dtos;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list manufacturers online.");
            return new List<ManufacturerDto>();
        }

        return new List<ManufacturerDto>();
    }

    public async Task SaveToOfflineAsync(IEnumerable<ManufacturerDto> items)
    {
        var domainItems = _mapper.Map<List<Manufacturer>>(items ?? new List<ManufacturerDto>());

        foreach (var item in domainItems)
        {
            await _database.InsertAsync(item);

            //var existing = await _database.FindAsync<Manufacturer>(item.Id);

            //if (existing == null)
            //{
            //    try
            //    {
            //        var response = await _database.InsertAsync(item);
            //    }
            //    catch (SQLiteException ex)
            //    {
            //        _logger.LogInformation(ex, $"Failed to insert manufacturer online. Delete the offline manufacturer.{item.Name}");
            //    }
            //}
            //else
            //{
            //    await _database.UpdateAsync(item);
            //}
        }
    }
    public async Task<bool> ClearTableAsync()
    {
        await InitAsync();

        if (_connectivity.IsOnline)
        {
            var isClearedOnline = await ClearTableOnlineAsync();

            if (!isClearedOnline)
            {
                throw new Exception("Failed to clear product table online.");
            }
        }

        await _database.DeleteAllAsync<Manufacturer>();
        await _database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = 'Manufacturer'");
        return true;
    }

    public async Task<bool> ClearTableOnlineAsync()
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/clear");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to drop manufacturer table online.");
            return false;
        }
    }

}
