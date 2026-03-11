using MiApp.Business;
using MiApp.Data;
using MiApp.Domain;
using System.Runtime.Versioning;

namespace MiApp.UI;

[SupportedOSPlatform("windows")]
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
[SupportedOSPlatform("windows")]
public class Form1 : Form
{
    private readonly User _currentUser;
    private readonly ProductService _productService;
    private readonly AuditService _auditService;
    private readonly DashboardService _dashboardService;
    private DataGridView dgv = new();
    private Button btnAdd = new();
    private Button btnDelete = new();
    private Button btnEntrada = new();
    private Button btnSalida = new();
    private Button btnLogout = new();
    private TextBox txtSearch = new();
    
    private Button btnAudit = new();
    private Button btnUsers = new();
    private Button btnDashboard = new();
    private Button btnExportProducts = new();
    private Button btnQR = new();

    private Button btnEdit = new();
    public Form1(User user)
    {
        _currentUser = user;

        var database = new DatabaseService();
        _productService = new ProductService(database);
        _auditService = new AuditService(database);
        _dashboardService = new DashboardService(_productService);

        CrearUI();
        AplicarPermisos();

        this.Load += async (s, e) => await CargarProductosAsync();
    }

    private void CrearUI()
    {
        this.Text = "MiApp - Inventario Profesional";
        this.Size = new Size(1200, 650);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(30, 30, 30);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 9);

        // =========================
        // TOP BAR
        // =========================

        Panel topBar = new Panel();
        topBar.Dock = DockStyle.Top;
        topBar.Height = 60;
        topBar.BackColor = Color.FromArgb(45, 45, 48);

        Label title = new Label();
        title.Text = "📦 MiApp Inventory System";
        title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        title.ForeColor = Color.White;
        title.AutoSize = true;
        title.Location = new Point(20, 18);

        topBar.Controls.Add(title);

        // =========================
        // TOOLBAR
        // =========================

        FlowLayoutPanel toolbar = new FlowLayoutPanel();
        toolbar.Dock = DockStyle.Top;
        toolbar.Height = 60;
        toolbar.Padding = new Padding(10);
        toolbar.BackColor = Color.FromArgb(37, 37, 38);
        toolbar.AutoSize = false;
        toolbar.WrapContents = false;

        // SEARCH BOX

        txtSearch.Width = 220;
        txtSearch.Margin = new Padding(10, 5, 10, 5);
        txtSearch.PlaceholderText = "🔍 Buscar producto...";
        txtSearch.BackColor = Color.FromArgb(45, 45, 48);
        txtSearch.ForeColor = Color.White;
        txtSearch.BorderStyle = BorderStyle.FixedSingle;
        txtSearch.KeyDown += TxtSearch_KeyDown;

        txtSearch.TextChanged += async (s, e) =>
        {
            await CargarProductosAsync(txtSearch.Text);
        };

        // =========================
        // BUTTON STYLE
        // =========================

        void StyleButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(0, 122, 204);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Height = 35;
            btn.Width = 110;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(28, 151, 234);
            };

            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(0, 122, 204);
            };
        }

        // =========================
        // BUTTONS
        // =========================

        btnQR.Text = "🔳 QR";
        btnQR.Click += BtnQR_Click;
        StyleButton(btnQR);

        btnAdd.Text = "➕ Agregar";
        btnAdd.Click += async (s, e) => await BtnAdd_ClickAsync();
        StyleButton(btnAdd);

        btnDelete.Text = "🗑 Eliminar";
        btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync();
        StyleButton(btnDelete);

        btnEntrada.Text = "📦 Entrada";
        btnEntrada.Click += async (s, e) => await BtnEntrada_ClickAsync();
        StyleButton(btnEntrada);

        btnSalida.Text = "📤 Salida";
        btnSalida.Click += async (s, e) => await BtnSalida_ClickAsync();
        StyleButton(btnSalida);

        btnUsers.Text = "👤 Usuarios";
        btnUsers.Click += BtnUsers_Click;
        StyleButton(btnUsers);

        btnAudit.Text = "📜 Auditoría";
        btnAudit.Click += BtnAudit_Click;
        StyleButton(btnAudit);

        btnDashboard.Text = "📊 Dashboard";
        btnDashboard.Click += BtnDashboard_Click;
        StyleButton(btnDashboard);

        btnLogout.Text = "🚪 Logout";
        btnLogout.Click += BtnLogout_Click;
        StyleButton(btnLogout);

        // =========================
        // ADD TO TOOLBAR
        // =========================

        toolbar.Controls.Add(btnQR);
        toolbar.Controls.Add(btnAdd);
        toolbar.Controls.Add(btnDelete);
        toolbar.Controls.Add(btnEntrada);
        toolbar.Controls.Add(btnSalida);
        toolbar.Controls.Add(txtSearch);
        toolbar.Controls.Add(btnUsers);
        toolbar.Controls.Add(btnAudit);
        toolbar.Controls.Add(btnDashboard);
        toolbar.Controls.Add(btnLogout);

        // =========================
        // DATAGRID
        // =========================

        dgv.Dock = DockStyle.Fill;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.ReadOnly = true;
        dgv.RowHeadersVisible = false;
        dgv.AllowUserToAddRows = false;

        dgv.BackgroundColor = Color.FromArgb(37, 37, 38);
        dgv.BorderStyle = BorderStyle.None;
        dgv.GridColor = Color.FromArgb(60, 60, 60);

        dgv.EnableHeadersVisualStyles = false;

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        dgv.DefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
        dgv.DefaultCellStyle.ForeColor = Color.White;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);

        dgv.RowTemplate.Height = 32;

        dgv.CellFormatting += Dgv_CellFormatting;

        // =========================
        // BOTTOM PANEL
        // =========================

        Panel bottomPanel = new Panel();
        bottomPanel.Dock = DockStyle.Bottom;
        bottomPanel.Height = 60;
        bottomPanel.BackColor = Color.FromArgb(37, 37, 38);

        btnExportProducts.Text = "⬇ Exportar CSV";
        btnExportProducts.Width = 150;
        btnExportProducts.Location = new Point(20, 12);
        btnExportProducts.Click += async (s, e) => await BtnExportProducts_ClickAsync();
        StyleButton(btnExportProducts);

        btnEdit.Text = "✏ Editar";
        btnEdit.Width = 100;
        btnEdit.Location = new Point(200, 12);
        btnEdit.Click += async (s, e) => await BtnEdit_ClickAsync();
        StyleButton(btnEdit);

        bottomPanel.Controls.Add(btnExportProducts);
        bottomPanel.Controls.Add(btnEdit);

        // =========================
        // ADD CONTROLS (ORDER IMPORTANT)
        // =========================

        this.Controls.Add(dgv);
        this.Controls.Add(bottomPanel);
        this.Controls.Add(toolbar);
        this.Controls.Add(topBar);
    }

    private void AplicarPermisos()
    {
        btnDelete.Enabled = PermissionService.CanDeleteProduct(_currentUser);
        btnAdd.Enabled = PermissionService.CanCreateProduct(_currentUser);
        btnAudit.Enabled = PermissionService.CanViewAudit(_currentUser);
        btnUsers.Enabled = _currentUser.Role == UserRole.Admin;
    }

    private async Task CargarProductosAsync(string? search = null)
    {
        dgv.DataSource = await _productService.GetProductsAsync(search);
    }

    private async Task BtnAdd_ClickAsync()
    {
        if (!PermissionService.CanCreateProduct(_currentUser))
            return;

        using var form = new ProductForm();

        if (form.ShowDialog() == DialogResult.OK && form.Product != null)
        {
            await _productService.AddProductAsync(form.Product);

            await _auditService.LogAsync(
                _currentUser.Id,
                "CREATE_PRODUCT",
                $"Producto creado: {form.Product.Name}"
            );

            await CargarProductosAsync();
        }
    }

    private async Task BtnDelete_ClickAsync()
    {
        if (!PermissionService.CanDeleteProduct(_currentUser))
            return;

        if (dgv.CurrentRow?.DataBoundItem is not Product p)
            return;

        await _productService.DeleteProductAsync(p.Id);

        await _auditService.LogAsync(
            _currentUser.Id,
            "DELETE_PRODUCT",
            $"Producto eliminado: {p.Name}"
        );

        await CargarProductosAsync();
    }

    private async Task BtnEntrada_ClickAsync()
    {
        if (dgv.CurrentRow?.DataBoundItem is not Product p)
            return;

        int cantidad = 1;

        await _productService.RegisterStockMovementAsync(p.Id, 1, cantidad);

        await _auditService.LogAsync(
            _currentUser.Id,
            "STOCK_ENTRY",
            $"Entrada stock: {p.Name} +{cantidad}"
        );

        await CargarProductosAsync();
    }

    private async Task BtnSalida_ClickAsync()
    {
        if (dgv.CurrentRow?.DataBoundItem is not Product p)
            return;

        int cantidad = 1;

        await _productService.RegisterStockMovementAsync(p.Id, 2, cantidad);

        await _auditService.LogAsync(
            _currentUser.Id,
            "STOCK_EXIT",
            $"Salida stock: {p.Name} -{cantidad}"
        );

        await CargarProductosAsync();
    }

    private async void BtnLogout_Click(object? sender, EventArgs e)
    {
        await _auditService.LogAsync(
            _currentUser.Id,
            "LOGOUT",
            "Cierre de sesión"
        );

        this.Close();
    }

    private void BtnAudit_Click(object? sender, EventArgs e)
    {
        if (!PermissionService.CanViewAudit(_currentUser))
        {
            MessageBox.Show("No tiene permisos para ver la auditoría.");
            return;
        }

        using var form = new AuditForm(_auditService);
        form.ShowDialog();
    }

    private void BtnUsers_Click(object? sender, EventArgs e)
    {
        using var form = new UsersForm();
        form.ShowDialog();
    }

    private void BtnDashboard_Click(object? sender, EventArgs e)
    {
        using var form = new DashboardForm(_dashboardService);
        form.ShowDialog();
    }

    private async Task BtnExportProducts_ClickAsync()
    {
        var products = await _productService.GetProductsAsync();

        var exporter = new CsvExportService();
        var csv = exporter.ExportProducts(products.ToList());

        using SaveFileDialog sfd = new();
        sfd.Filter = "CSV files (*.csv)|*.csv";
        sfd.FileName = "productos.csv";

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            await File.WriteAllTextAsync(sfd.FileName, csv);
            MessageBox.Show("Productos exportados correctamente.");
        }
    }

    private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        if (dgv.Rows[e.RowIndex].DataBoundItem is Product p &&
            p.StockActual <= p.StockMinimo)
        {
            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
            dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
        }
    }

    private async Task BtnEdit_ClickAsync()
    {
        if (dgv.CurrentRow?.DataBoundItem is not Product p)
            return;

        using var form = new ProductForm(p);

        if (form.ShowDialog() == DialogResult.OK && form.Product != null)
        {
            await _productService.UpdateProductAsync(form.Product);

            await _auditService.LogAsync(
                _currentUser.Id,
                "UPDATE_PRODUCT",
                $"Producto actualizado: {form.Product.Name}"
            );

            await CargarProductosAsync();
        }
    }

    private void BtnQR_Click(object? sender, EventArgs e)
    {
        if (dgv.CurrentRow?.DataBoundItem is not Product p)
            return;

        var qrService = new QrService();

        byte[] qrBytes = qrService.GenerateQr(p.Id.ToString());

        using var ms = new MemoryStream(qrBytes);
        var img = new Bitmap(ms);

        Form qrForm = new Form();
        qrForm.Text = $"QR - {p.Name}";
        qrForm.Size = new Size(320, 320);
        qrForm.StartPosition = FormStartPosition.CenterParent;

        PictureBox pb = new PictureBox();
        pb.Dock = DockStyle.Fill;
        pb.SizeMode = PictureBoxSizeMode.Zoom;
        pb.Image = img;

        qrForm.Controls.Add(pb);

        qrForm.ShowDialog();
    }

    private async void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            // intentar buscar por ID (QR)
            if (int.TryParse(txtSearch.Text, out int id))
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product != null)
                {
                    dgv.DataSource = new List<Product> { product };
                    return;
                }
            }

            // si no es ID buscar por texto
            await CargarProductosAsync(txtSearch.Text);
        }
    }
    
}