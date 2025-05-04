using ArcsomAssetManagement.Api.Data;
using ArcsomAssetManagement.Api.Models;
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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var stoppingToken = source.Token;

        var products = await _context.Products.AsNoTracking().ToListAsync(stoppingToken);
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

        var product = new Product
        {
            Name = request.Name,
            Manufacturer = request.Manufacturer
        };

        await _context.Products.AddAsync(product, stoppingToken);
        await _context.SaveChangesAsync(stoppingToken);

        return Ok(product);
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
