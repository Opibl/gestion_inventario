using MiApp.UI;
using MiApp.Domain;
public class ProductForm : Form
{
    public Product? Product { get; private set; }

    TextBox txtName = new();
    TextBox txtBarcode = new();
    TextBox txtPrice = new();
    TextBox txtStock = new();
    TextBox txtStockMin = new();
    TextBox txtCategory = new();
    TextBox txtEco = new();
    TextBox txtSocial = new();
    Button btnSave = new();

    // NUEVO PRODUCTO
    public ProductForm()
    {
        this.Text = "Nuevo Producto";
        this.Size = new Size(350, 500);
        this.StartPosition = FormStartPosition.CenterParent;

        Label lblName = new() { Text = "Nombre", Top = 20, Left = 20 };
        txtName.SetBounds(20, 40, 280, 25);

        Label lblBarcode = new() { Text = "Barcode", Top = 70, Left = 20 };
        txtBarcode.SetBounds(20, 90, 280, 25);

        Label lblPrice = new() { Text = "Precio", Top = 120, Left = 20 };
        txtPrice.SetBounds(20, 140, 280, 25);

        Label lblStock = new() { Text = "Stock Inicial", Top = 170, Left = 20 };
        txtStock.SetBounds(20, 190, 280, 25);

        Label lblStockMin = new() { Text = "Stock Mínimo", Top = 220, Left = 20 };
        txtStockMin.SetBounds(20, 240, 280, 25);

        Label lblCategory = new() { Text = "Categoría", Top = 270, Left = 20 };
        txtCategory.SetBounds(20, 290, 280, 25);

        Label lblEco = new() { Text = "Eco Score (0-100)", Top = 320, Left = 20 };
        txtEco.SetBounds(20, 340, 280, 25);

        Label lblSocial = new() { Text = "Social Score (0-100)", Top = 370, Left = 20 };
        txtSocial.SetBounds(20, 390, 280, 25);

        btnSave.Text = "Guardar";
        btnSave.SetBounds(20, 430, 280, 35);
        btnSave.Click += BtnSave_Click;

        this.Controls.AddRange(new Control[]
        {
            lblName, txtName,
            lblBarcode, txtBarcode,
            lblPrice, txtPrice,
            lblStock, txtStock,
            lblStockMin, txtStockMin,
            lblCategory, txtCategory,
            lblEco, txtEco,
            lblSocial, txtSocial,
            btnSave
        });
    }

    // EDITAR PRODUCTO
    public ProductForm(Product product) : this()
    {
        this.Text = "Editar Producto";

        txtName.Text = product.Name;
        txtBarcode.Text = product.Barcode;
        txtPrice.Text = product.Price.ToString();
        txtStock.Text = product.StockActual.ToString();
        txtStockMin.Text = product.StockMinimo.ToString();
        txtCategory.Text = product.Category;
        txtEco.Text = product.EcoScore?.ToString();
        txtSocial.Text = product.SocialScore?.ToString();

        Product = product;

        // evitar cambiar stock inicial al editar
        txtStock.Enabled = false;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Nombre requerido");
            return;
        }

        if (!decimal.TryParse(txtPrice.Text, out decimal price))
        {
            MessageBox.Show("Precio inválido");
            return;
        }

        if (!int.TryParse(txtStock.Text, out int stock))
        {
            MessageBox.Show("Stock inválido");
            return;
        }

        if (!int.TryParse(txtStockMin.Text, out int stockMin))
        {
            MessageBox.Show("Stock mínimo inválido");
            return;
        }

        if (!int.TryParse(txtEco.Text, out int eco) || eco < 0 || eco > 100)
        {
            MessageBox.Show("Eco Score inválido (0-100)");
            return;
        }

        if (!int.TryParse(txtSocial.Text, out int social) || social < 0 || social > 100)
        {
            MessageBox.Show("Social Score inválido (0-100)");
            return;
        }

        if (Product == null)
            Product = new Product();

        Product.Name = txtName.Text;
        Product.Barcode = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text;
        Product.Price = (int)price;
        Product.StockActual = stock;
        Product.StockMinimo = stockMin;
        Product.Category = txtCategory.Text;
        Product.EcoScore = eco;
        Product.SocialScore = social;

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}