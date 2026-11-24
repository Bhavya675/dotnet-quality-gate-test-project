using System.Security.Claims;
using DotNetQualityGate.Api.Contracts;
using DotNetQualityGate.Api.Data;
using DotNetQualityGate.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetQualityGate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProductsController(AppDbContext db, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var products = await db.Products
            .Where(p => p.OwnerId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var product = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 100)
            return BadRequest("Product name must contain 1-100 characters.");
        if (request.Price <= 0 || request.Price > 100000)
            return BadRequest("Price must be greater than zero and at most 100000.");

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
