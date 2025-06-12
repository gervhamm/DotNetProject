using ArcsomAssetManagement.Api.Data;
using ArcsomAssetManagement.Api.DTOs.Business;
using ArcsomAssetManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcsomAssetManagement.Api.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssetController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssetController> _logger;

    public AssetController(ApplicationDbContext context, ILogger<AssetController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("Paged")]
    public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 3, string filter = "")
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        filter = filter.Trim().ToLowerInvariant();

        var assets = await _context.Assets.AsNoTracking()
            .Include(p => p.Product)
            .Select(p => new AssetDto
            {
                Id = p.Id,
                Name = p.Name,
                ProductDto = new ProductDto
                {
                    Id = p.Product.Id,
                    Name = p.Product.Name,
                }
            })
            .Where(p => string.IsNullOrEmpty(filter) ||
                p.Name.Contains(filter))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(stoppingToken);

        var totalAssets = await _context.Assets.CountAsync(stoppingToken);

        if (!assets.Any())
        {
            return NotFound("Not Found");
        }

        Response.Headers.Add("X-Total-Count", totalAssets.ToString());

        return Ok(assets);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var assets = await _context.Assets.AsNoTracking()
            .Include(p => p.Product)
            .Select(p => new AssetDto
            {
                Id = p.Id,
                Name = p.Name,
                ProductDto = new ProductDto
                {
                    Id = p.Product.Id,
                    Name = p.Product.Name,
                }
            })
            .ToListAsync(stoppingToken);
        if (!assets.Any())
        {
            return NotFound("Not Found");
        }
        return Ok(assets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] ulong id)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var asset = await _context.Assets.AsNoTracking()
            .Include(p => p.Product)
            .Select(p => new AssetDto
            {
                Id = p.Id,
                Name = p.Name,
                ProductDto = new ProductDto
                {
                    Id = p.Product.Id,
                    Name = p.Product.Name,
                }
            })
            .FirstOrDefaultAsync(p => p.Id == id, stoppingToken);
        if (asset == null)
        {
            return NotFound("Not Found");
        }
        return Ok(asset);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AssetDto request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var product = await _context.Products
            .FirstOrDefaultAsync(m => m.Id == request.ProductDto.Id, stoppingToken);

        var asset = new Asset
        {
            Name = request.Name,
            Product = product
        };
        try
        {
            await _context.Assets.AddAsync(asset, stoppingToken);
            await _context.SaveChangesAsync(stoppingToken);
            return Ok(asset.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error adding asset");
            return BadRequest("An error occurred while adding the asset.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update([FromRoute] ulong id, [FromBody] AssetDto request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        try
        {
            var asset = await _context.Assets.Include(m => m.Product)
                .FirstOrDefaultAsync(c => c.Id == id, stoppingToken);
            if (asset == null)
            {
                return NotFound("Not Found");
            }
            asset.Name = request.Name;
            asset.Product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == request.ProductDto.Id, stoppingToken);

            await _context.SaveChangesAsync(stoppingToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(ulong id)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var asset = await _context.Assets.FirstOrDefaultAsync(c => c.Id == id, stoppingToken);

        if (asset == null)
        {
            return NotFound("Not Found");
        }

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync(stoppingToken);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var assets = await _context.Assets.ToListAsync();
        _context.Assets.RemoveRange(assets);
        await _context.SaveChangesAsync(stoppingToken);
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Assets', RESEED, 0)");

        return NoContent();
    }
}
