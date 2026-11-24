namespace DotNetQualityGate.Api.Models;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public required string OwnerId { get; set; }
}
