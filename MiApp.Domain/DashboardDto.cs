namespace MiApp.Domain;

public class DashboardDto
{
    public int TotalProductos { get; set; }
    public int StockBajo { get; set; }
    public decimal ValorInventario { get; set; }
    public double EcoPromedio { get; set; }
    public double SocialPromedio { get; set; }
}