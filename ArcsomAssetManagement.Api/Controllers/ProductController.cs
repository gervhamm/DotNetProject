using ArcsomAssetManagement.Client.Data;
using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcsomAssetManagement.Client.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductController> _logger;

    public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
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

        var products = await _context.Products.AsNoTracking()
            .Include(p => p.Manufacturer)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ManufacturerDto = new ManufacturerDto
                {
                    Id = p.Manufacturer.Id,
                    Name = p.Manufacturer.Name,
                    Contact = p.Manufacturer.Contact,
                }


            })
            .ToListAsync(stoppingToken);
        if (products == null)
        {
            return NotFound("Not Found");
        }
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] ulong id)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var product = await _context.Products.AsNoTracking()
            .Include(p => p.Manufacturer)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ManufacturerDto = new ManufacturerDto
                {
                    Id = p.Manufacturer.Id,
                    Name = p.Manufacturer.Name,
                    Contact = p.Manufacturer.Contact,
                }


            })
            .FirstOrDefaultAsync(p => p.Id == id, stoppingToken);
        if (product == null)
        {
            return NotFound("Not Found");
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Product request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturer = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == request.Manufacturer.Id, stoppingToken);

        request.Manufacturer = null;

        if (manufacturer is null)
        {
            return BadRequest("Manufacturer not found");
        }

        var product = new Product
        {
            Name = request.Name,
            Manufacturer = manufacturer
        };

        var resutl = await _context.Products.AddAsync(product, stoppingToken);
        var resultSave = await _context.SaveChangesAsync(stoppingToken);

        return Ok(product.Id);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update([FromRoute] ulong id, [FromBody] Product request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var product = await _context.Products.FirstOrDefaultAsync(c => c.Id == id, stoppingToken);
        if (product == null)
        {
            return NotFound("Not Found");
        }
        product.Name = request.Name;
        product.Manufacturer = request.Manufacturer;
        await _context.SaveChangesAsync(stoppingToken);

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(ulong id)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var product = await _context.Products.FirstOrDefaultAsync(c => c.Id == id, stoppingToken);

        if (product == null)
        {
            return NotFound("Not Found");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(stoppingToken);

        return NoContent();

    }
}
