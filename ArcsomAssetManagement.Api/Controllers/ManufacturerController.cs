using ArcsomAssetManagement.Client.Data;
using ArcsomAssetManagement.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Get()
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturers = await _context.Manufacturers.AsNoTracking().ToListAsync(stoppingToken);
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

        var manufacturer = await _context.Manufacturers.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, stoppingToken);
        if (manufacturer == null)
        {
            return NotFound("Not Found");
        }
        return Ok(manufacturer);
    }
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Manufacturer request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturer = new Manufacturer
        {
            Name = request.Name,
            Contact = request.Contact,
            Products = request.Products
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
    public async Task<IActionResult> Update([FromRoute] ulong id, [FromBody] Manufacturer request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturer = await _context.Manufacturers.FirstOrDefaultAsync(c => c.Id == id, stoppingToken);
        if (manufacturer == null)
        {
            return NotFound("Not Found");
        }
        manufacturer.Name = request.Name;
        manufacturer.Contact = request.Contact;
        manufacturer.Products = request.Products;

        await _context.SaveChangesAsync(stoppingToken);

        return Ok(manufacturer);
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

