using Xunit;
using MiApp.Business;
using MiApp.Domain;

namespace MiApp.Tests;

public class ProductServiceTests
{
    private readonly FakeDatabaseService _fakeDb;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _fakeDb = new FakeDatabaseService();
        _service = new ProductService(_fakeDb);
    }

    [Fact]
    public async Task AddProduct_Should_Add_Product()
    {
        var product = new Product
        {
            Name = "Laptop",
            Price = 1000,
            StockActual = 10,
            StockMinimo = 2,
            Category = "Tech"
        };

        await _service.AddProductAsync(product);

        var products = await _service.GetProductsAsync();

        Assert.Contains(products, p => p.Name == "Laptop");
    }

    [Fact]
    public async Task DeleteProduct_Should_Remove_Product()
    {
        var product = new Product
        {
            Name = "Mouse",
            Price = 20,
            StockActual = 5,
            StockMinimo = 1
        };

        await _service.AddProductAsync(product);

        var list = await _service.GetProductsAsync();
        var p = list.First();

        await _service.DeleteProductAsync(p.Id);

        var after = await _service.GetProductsAsync();

        Assert.DoesNotContain(after, x => x.Id == p.Id);
    }

    [Fact]
    public async Task UpdateProduct_Should_Update_Data()
    {
        var product = new Product
        {
            Name = "Keyboard",
            Price = 50,
            StockActual = 5,
            StockMinimo = 1
        };

        await _service.AddProductAsync(product);

        var p = (await _service.GetProductsAsync()).First();

        p.Name = "Mechanical Keyboard";

        await _service.UpdateProductAsync(p);

        var updated = (await _service.GetProductsAsync()).First();

        Assert.Equal("Mechanical Keyboard", updated.Name);
    }

    [Fact]
    public async Task StockEntry_Should_Increase_Stock()
    {
        var product = new Product
        {
            Name = "SSD",
            Price = 150,
            StockActual = 10,
            StockMinimo = 2
        };

        await _service.AddProductAsync(product);

        var p = (await _service.GetProductsAsync()).First();

        await _service.RegisterStockMovementAsync(p.Id, 1, 5);

        var updated = (await _service.GetProductsAsync()).First();

        Assert.Equal(15, updated.StockActual);
    }

    [Fact]
    public async Task StockExit_Should_Decrease_Stock()
    {
        var product = new Product
        {
            Name = "RAM",
            Price = 80,
            StockActual = 10,
            StockMinimo = 2
        };

        await _service.AddProductAsync(product);

        var p = (await _service.GetProductsAsync()).First();

        await _service.RegisterStockMovementAsync(p.Id, 2, 3);

        var updated = (await _service.GetProductsAsync()).First();

        Assert.Equal(7, updated.StockActual);
    }
}