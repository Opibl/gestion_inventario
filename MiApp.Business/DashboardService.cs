using MiApp.Domain;

namespace MiApp.Business;

public class DashboardService
{
    private readonly ProductService _productService;

    public DashboardService(ProductService productService)
    {
        _productService = productService;
    }

    public async Task<DashboardMetrics> GetMetricsAsync()
    {
        var products = (await _productService.GetProductsAsync()).ToList();

        return new DashboardMetrics
        {
            TotalProductos = products.Count,
            StockBajo = products.Count(p => p.StockActual <= p.StockMinimo),
            ValorInventario = products.Sum(p => p.Price * p.StockActual),

            PromedioEco = products.Any(p => p.EcoScore.HasValue)
                ? products.Where(p => p.EcoScore.HasValue)
                        .Average(p => p.EcoScore!.Value)
                : 0,

            PromedioSocial = products.Any(p => p.SocialScore.HasValue)
                ? products.Where(p => p.SocialScore.HasValue)
                        .Average(p => p.SocialScore!.Value)
                : 0
        };
    }
}

public class DashboardMetrics
{
    public int TotalProductos { get; set; }
    public int StockBajo { get; set; }
    public decimal ValorInventario { get; set; }
    public double PromedioEco { get; set; }
    public double PromedioSocial { get; set; }
}