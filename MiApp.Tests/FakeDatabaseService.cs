using MiApp.Data;
using MiApp.Domain;

namespace MiApp.Tests;

public class FakeDatabaseService : DatabaseService
{
    private readonly List<Product> _products = new();
    private int _id = 1;

    public FakeDatabaseService() : base(true)
    {
    }

    public override Task<List<Product>> GetProductsAsync(string? search = null)
    {
        return Task.FromResult(_products.ToList());
    }

    public override Task AddProductAsync(Product p)
    {
        p.Id = _id++;
        _products.Add(p);
        return Task.CompletedTask;
    }

    public override Task DeleteProductAsync(int id)
    {
        _products.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }

    public override Task UpdateProductAsync(Product p)
    {
        var existing = _products.First(x => x.Id == p.Id);
        existing.Name = p.Name;
        existing.Price = p.Price;
        existing.StockActual = p.StockActual;
        existing.StockMinimo = p.StockMinimo;

        return Task.CompletedTask;
    }

    public override Task RegisterStockMovementAsync(int productId, int type, int quantity, string userName)
    {
        var product = _products.First(p => p.Id == productId);

        if (type == 1)
            product.StockActual += quantity;
        else
            product.StockActual -= quantity;

        return Task.CompletedTask;
    }
}