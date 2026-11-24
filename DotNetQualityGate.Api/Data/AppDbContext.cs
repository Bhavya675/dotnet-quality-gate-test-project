using DotNetQualityGate.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetQualityGate.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
