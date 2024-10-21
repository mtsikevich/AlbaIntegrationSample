using Microsoft.EntityFrameworkCore;

namespace Api;

public class ProductDb(DbContextOptions<ProductDb> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

