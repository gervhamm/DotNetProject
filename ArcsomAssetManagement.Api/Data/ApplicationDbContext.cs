using ArcsomAssetManagement.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArcsomAssetManagement.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, Role, ulong>(options)
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Manufacturer> Manufacturers { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
}
