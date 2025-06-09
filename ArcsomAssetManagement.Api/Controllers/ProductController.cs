using ArcsomAssetManagement.Api.Data;
using ArcsomAssetManagement.Api.DTOs.Business;
using ArcsomAssetManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcsomAssetManagement.Api.Controllers;

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

    [HttpGet("Paged")]
    public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 3, string filter = "")
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        filter = filter.Trim().ToLowerInvariant();

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
            .Where(p => string.IsNullOrEmpty(filter) ||
                p.Name.Contains(filter))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(stoppingToken);

        var totalProducts = await _context.Products.CountAsync(stoppingToken);

        if (products == null)
        {
            return NotFound("Not Found");
        }

        Response.Headers.Add("X-Total-Count", totalProducts.ToString());

        return Ok(products);
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

    [Authorize]
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
    public async Task<IActionResult> Add([FromBody] ProductDto request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var manufacturer = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == request.ManufacturerDto.Id, stoppingToken);

        var product = new Product
        {
            Name = request.Name,
            Manufacturer = manufacturer
        };
        try
        {
            await _context.Products.AddAsync(product, stoppingToken);
            await _context.SaveChangesAsync(stoppingToken);
            return Ok(product.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error adding product");
            return BadRequest("An error occurred while adding the product.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update([FromRoute] ulong id, [FromBody] ProductDto request)
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        try
        {
            var product = await _context.Products.Include(m => m.Manufacturer)
                .FirstOrDefaultAsync(c => c.Id == id, stoppingToken);
            if (product == null)
            {
                return NotFound("Not Found");
            }
            product.Name = request.Name;
            product.Manufacturer = await _context.Manufacturers
                .FirstOrDefaultAsync(m => m.Id == request.ManufacturerDto.Id, stoppingToken);

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
