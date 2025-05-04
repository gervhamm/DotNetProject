using ArcsomAssetManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArcsomAssetManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
