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
public class ManufacturerController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManufacturerController> _logger;

    public ManufacturerController(ApplicationDbContext context, ILogger<ManufacturerController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("Paged")]
    public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 3, string filter = "", bool desc = false)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        filter = filter.Trim().ToLowerInvariant();

        var manufacturersQuery = _context.Manufacturers.AsNoTracking();

        if (!string.IsNullOrEmpty(filter))
        {
            manufacturersQuery = manufacturersQuery.Where(p =>
                p.Name.Contains(filter) ||
                p.Contact.Contains(filter));
        }

        manufacturersQuery = desc ? manufacturersQuery.OrderByDescending(p => p.Name) : manufacturersQuery.OrderBy(p => p.Name);

        var manufacturers = await manufacturersQuery
            .Select(p => new ManufacturerDto
            {
                Id = p.Id,
                Name = p.Name,
                Contact = p.Contact,
                ProductDtos = null
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(stoppingToken);

        var totalManufacturers = await _context.Manufacturers.CountAsync(stoppingToken);

        if (!manufacturers.Any())
        {
            return NotFound("Not Found");
        }

        Response.Headers.Add("X-Total-Count", totalManufacturers.ToString());

        return Ok(manufacturers);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturers = await _context.Manufacturers.AsNoTracking()
            .Select(p => new ManufacturerDto
            {
                Id = p.Id,
                Name = p.Name,
                Contact = p.Contact,
                ProductDtos = null
            })
            .ToListAsync(stoppingToken);
        if (!manufacturers.Any())
        {
            return NotFound("Not Found");
        }
        return Ok(manufacturers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] ulong id)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var products = await _context.Products.AsNoTracking()
            .Include(p => p.Manufacturer)
            .Where(p => p.Manufacturer.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ManufacturerDto = new ManufacturerDto
                {
                    Id = p.Manufacturer.Id,
                    Name = p.Manufacturer.Name,
                    Contact = p.Manufacturer.Contact
                }
            })
            .ToListAsync(stoppingToken);

        var manufacturer = await _context.Manufacturers.AsNoTracking()
            .Select(p => new ManufacturerDto
            {
                Id = p.Id,
                Name = p.Name,
                Contact = p.Contact,
                ProductDtos = products
            })
            .FirstOrDefaultAsync(p => p.Id == id, stoppingToken);

        if (manufacturer == null)
        {
            return NotFound("Not Found");
        }
        return Ok(manufacturer);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ManufacturerDto request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturer = new Manufacturer
        {
            Name = request.Name,
            Contact = request.Contact,
            Products = request.ProductDtos.Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Manufacturer = null
            }).ToList()
        };
        try
        {
            await _context.Manufacturers.AddAsync(manufacturer, stoppingToken);
            await _context.SaveChangesAsync(stoppingToken);
            return CreatedAtAction(nameof(Get), new { id = manufacturer.Id }, manufacturer);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error adding manufacturer");
            return BadRequest("An error occurred while adding the manufacturer.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update([FromRoute] ulong id, [FromBody] ManufacturerDto request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        try
        {
            var manufacturer = await _context.Manufacturers.Include(p => p.Products)
                .FirstOrDefaultAsync(c => c.Id == id, stoppingToken);
            if (manufacturer == null)
            {
                return NotFound("Not Found");
            }
            manufacturer.Name = request.Name;
            manufacturer.Contact = request.Contact;

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

        var manufacturer = await _context.Manufacturers.Include(p => p.Products).FirstOrDefaultAsync(c => c.Id == id, stoppingToken);

        if (manufacturer == null)
        {
            return NotFound("Not Found");
        }

        if (manufacturer.Products != null)
        {
            _context.Products.RemoveRange(manufacturer.Products);
        }

        _context.Manufacturers.Remove(manufacturer);
        await _context.SaveChangesAsync(stoppingToken);

        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturers = await _context.Manufacturers.ToListAsync();
        _context.Manufacturers.RemoveRange(manufacturers);
        await _context.SaveChangesAsync(stoppingToken);
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Manufacturers', RESEED, 0)");

        return NoContent();
    }
}

