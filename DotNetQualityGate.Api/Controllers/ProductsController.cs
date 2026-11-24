// INTENTIONALLY VULNERABLE TEST PATCH. Do not deploy.
using DotNetQualityGate.Api.Contracts;
using DotNetQualityGate.Api.Data;
using DotNetQualityGate.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetQualityGate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(AppDbContext db, ILogger<ProductsController> logger) : ControllerBase
{
    // BLOCKER: SQL injection through user-controlled raw SQL.
    [HttpGet("search")]
    public async Task<IActionResult> Search(string term, CancellationToken cancellationToken)
    {
        var sql = $"SELECT * FROM Products WHERE Name LIKE '%{term}%'";
        var products = await db.Products.FromSqlRaw(sql).ToListAsync(cancellationToken);
        return Ok(products);
    }

    // BLOCKER: Missing authorization and ownership check; any caller can read any product.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    // BLOCKER: Missing validation allows invalid data into persistent storage.
    [HttpPost]
    public async Task<ActionResult<Product>> Create(
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            OwnerId = "admin"
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(product);
    }

    // BLOCKER: Unbounded result set can cause excessive database and memory usage.
    [HttpGet("all")]
    public async Task<IActionResult> GetEverything(CancellationToken cancellationToken)
    {
        return Ok(await db.Products.ToListAsync(cancellationToken));
    }
}
