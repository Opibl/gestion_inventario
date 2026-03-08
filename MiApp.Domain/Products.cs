namespace MiApp.Domain
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string NormalizedName =>
            Name.ToLower().Trim();

        public string? Barcode { get; set; }

        public int Price { get; set; }

        public int? EcoScore { get; set; }
        public int? SocialScore { get; set; }

        public string? Category { get; set; }

        public string? ImageUrl { get; set; }

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; }
    }
}