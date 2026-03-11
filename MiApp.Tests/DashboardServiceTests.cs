using Xunit;
using MiApp.Business;
using MiApp.Domain;

namespace MiApp.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task Metrics_Should_Return_TotalProducts()
    {
        var fakeDb = new FakeDatabaseService();
        var productService = new ProductService(fakeDb);

        await productService.AddProductAsync(new Product
        {
            Name = "Producto A",
            Price = 10,
            StockActual = 5,
            StockMinimo = 1
        });

        await productService.AddProductAsync(new Product
        {
            Name = "Producto B",
            Price = 20,
            StockActual = 3,
            StockMinimo = 1
        });

        var dashboard = new DashboardService(productService);

        var metrics = await dashboard.GetMetricsAsync();

        Assert.Equal(2, metrics.TotalProductos);
    }

    [Fact]
    public async Task Metrics_Should_Detect_LowStock()
    {
        var fakeDb = new FakeDatabaseService();
        var productService = new ProductService(fakeDb);

        await productService.AddProductAsync(new Product
        {
            Name = "USB",
            Price = 10,
            StockActual = 1,
            StockMinimo = 5
        });

        var dashboard = new DashboardService(productService);

        var metrics = await dashboard.GetMetricsAsync();

        Assert.Equal(1, metrics.StockBajo);
    }

    [Fact]
    public async Task Metrics_Should_Calculate_Inventory_Value()
    {
        var fakeDb = new FakeDatabaseService();
        var productService = new ProductService(fakeDb);

        await productService.AddProductAsync(new Product
        {
            Name = "SSD",
            Price = 100,
            StockActual = 2,
            StockMinimo = 1
        });

        var dashboard = new DashboardService(productService);

        var metrics = await dashboard.GetMetricsAsync();

        Assert.Equal(200, metrics.ValorInventario);
    }
}