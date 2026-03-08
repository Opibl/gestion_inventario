using MiApp.Data;
using MiApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiApp.Business
{
    public class ProductService
    {
        private readonly DatabaseService _db;

        public ProductService(DatabaseService db)
        {
            _db = db;
        }

        // ========================= GET PRODUCTS =========================

        public async Task<List<Product>> GetProductsAsync(string? search = null)
        {
            return await _db.GetProductsAsync(search);
        }

        // ========================= GET PRODUCT BY ID =========================

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var products = await _db.GetProductsAsync();
            return products.FirstOrDefault(p => p.Id == id);
        }

        // ========================= ADD PRODUCT =========================

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new Exception("El nombre es obligatorio.");

            if (product.Price <= 0)
                throw new Exception("El precio debe ser mayor a 0.");

            if (product.StockMinimo < 0)
                throw new Exception("Stock mínimo inválido.");

            if (product.StockActual < 0)
                throw new Exception("Stock inválido.");

            var products = await _db.GetProductsAsync();

            if (products.Any(p => p.Name.ToLower() == product.Name.ToLower()))
                throw new Exception("Ya existe un producto con ese nombre.");

            if (!string.IsNullOrWhiteSpace(product.Barcode))
            {
                if (products.Any(p => p.Barcode == product.Barcode))
                    throw new Exception("Ese código de barras ya está registrado.");
            }

            // 🔹 Validar EcoScore
            if (product.EcoScore < 0 || product.EcoScore > 100)
                throw new Exception("EcoScore inválido (0-100).");

            // 🔹 Validar SocialScore
            if (product.SocialScore < 0 || product.SocialScore > 100)
                throw new Exception("SocialScore inválido (0-100).");

            await _db.AddProductAsync(product);
        }

        // ========================= DELETE PRODUCT =========================

        public async Task DeleteProductAsync(int id)
        {
            await _db.DeleteProductAsync(id);
        }

        // ========================= LOW STOCK =========================

        public async Task<List<Product>> GetLowStockAsync()
        {
            var products = await _db.GetProductsAsync();

            return products
                .Where(p => p.StockActual <= p.StockMinimo)
                .ToList();
        }


    // ========================= UPDATE PRODUCT =========================

    public async Task UpdateProductAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new Exception("El nombre es obligatorio.");

        if (product.Price <= 0)
            throw new Exception("El precio debe ser mayor a 0.");

        if (product.StockMinimo < 0)
            throw new Exception("Stock mínimo inválido.");

        if (product.EcoScore < 0 || product.EcoScore > 100)
            throw new Exception("EcoScore inválido (0-100).");

        if (product.SocialScore < 0 || product.SocialScore > 100)
            throw new Exception("SocialScore inválido (0-100).");

        await _db.UpdateProductAsync(product);
    }

    // ========================= DASHBOARD METRICS =========================

        public async Task<(int total, double avgEco, int lowEco)> GetMetricsAsync()
        {
            return await _db.GetMetricsAsync();
        }

        // ========================= DASHBOARD FULL METRICS =========================

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var products = await _db.GetProductsAsync();

            var totalProductos = products.Count;

            var stockBajo = products
                .Count(p => p.StockActual <= p.StockMinimo);

            var valorInventario = products
                .Sum(p => p.Price * p.StockActual);

            var ecoPromedio = products.Any()
                ? products.Average(p => p.EcoScore ?? 0)
                : 0;

            var socialPromedio = products.Any()
                ? products.Average(p => p.SocialScore ?? 0)
                : 0;

            return new DashboardDto
            {
                TotalProductos = totalProductos,
                StockBajo = stockBajo,
                ValorInventario = valorInventario,
                EcoPromedio = ecoPromedio,
                SocialPromedio = socialPromedio
            };
        }

        // ========================= REGISTER STOCK MOVEMENT =========================

        public async Task RegisterStockMovementAsync(int productId, int movementType, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Cantidad inválida.");

            if (movementType != 1 && movementType != 2)
                throw new Exception("Tipo de movimiento inválido.");

            var products = await _db.GetProductsAsync();
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                throw new Exception("Producto no encontrado.");

            // 🔹 evitar stock negativo
            if (movementType == 2 && product.StockActual < quantity)
                throw new Exception("Stock insuficiente.");

            await _db.RegisterStockMovementAsync(productId, movementType, quantity, "admin");
        }
    }
}