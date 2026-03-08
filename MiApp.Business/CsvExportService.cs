using System.Text;
using MiApp.Domain;

namespace MiApp.Business;

public class CsvExportService
{
    public string ExportProducts(List<Product> products)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Id,Nombre,Barcode,Precio,StockActual,StockMinimo,Categoria,EcoScore,SocialScore");

        foreach (var p in products)
        {
            sb.AppendLine($"{p.Id}," +
                          $"{Escape(p.Name)}," +
                          $"{Escape(p.Barcode)}," +
                          $"{p.Price}," +
                          $"{p.StockActual}," +
                          $"{p.StockMinimo}," +
                          $"{Escape(p.Category)}," +
                          $"{p.EcoScore}," +
                          $"{p.SocialScore}");
        }

        return sb.ToString();
    }

    public string ExportAudit(List<(string Username, string Action, string Description, DateTime CreatedAt)> logs)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Usuario,Accion,Descripcion,Fecha");

        foreach (var log in logs)
        {
            sb.AppendLine($"{Escape(log.Username)}," +
                        $"{Escape(log.Action)}," +
                        $"{Escape(log.Description)}," +
                        $"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return sb.ToString();
    }

    private string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}