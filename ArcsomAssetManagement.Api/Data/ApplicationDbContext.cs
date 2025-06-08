using ArcsomAssetManagement.Client.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcsomAssetManagement.Client.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Manufacturer> Manufacturers { get; set; } = null!;
}
