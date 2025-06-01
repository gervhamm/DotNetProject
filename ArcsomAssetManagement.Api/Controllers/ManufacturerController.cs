using ArcsomAssetManagement.Client.Data;
using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Threading;

namespace ArcsomAssetManagement.Client.Controllers;

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

    [HttpGet]
    [Route("api/[controller]/Paged")]
    public async Task<IActionResult> Get([FromRoute] int pageNumber=1, int pageSize = 3)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturers = await _context.Manufacturers.AsNoTracking()
            .Skip((pageNumber - 1)* pageSize)
            .Take(pageSize)
            .Select(p => new ManufacturerDto
            {
                Id = p.Id,
                Name = p.Name,
                Contact = p.Contact,
                ProductDtos = null
            })
            .ToListAsync(stoppingToken);

        var totalManufacturers = await _context.Manufacturers.CountAsync(stoppingToken);

        if (manufacturers == null)
        {
            return NotFound("Not Found");
        }
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
        if (manufacturers == null)
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

        var manufacturers = await _context.Manufacturers.AsNoTracking()
            .Select(p => new ManufacturerDto
            {
                Id = p.Id,
                Name = p.Name,
                Contact = p.Contact,
                ProductDtos = products
            })
            .FirstOrDefaultAsync(p => p.Id == id, stoppingToken);

        if (manufacturers == null)
        {
            return NotFound("Not Found");
        }
        return Ok(manufacturers);
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
}

