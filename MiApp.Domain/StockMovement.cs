namespace MiApp.Domain
{
    public enum MovementType
    {
        Entrada = 1,
        Salida = 2,
        Ajuste = 3
    }

    public class StockMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public MovementType TipoMovimiento { get; set; }

        public int Cantidad { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Usuario { get; set; } = string.Empty;
    }
}