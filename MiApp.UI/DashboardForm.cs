using MiApp.Business;
using System.Drawing.Drawing2D;

namespace MiApp.UI;

public class DashboardForm : Form
{
    private readonly DashboardService _dashboardService;

    private Panel panelBars = new();
    private Panel panelPie = new();

    private Label lblTotal = new();
    private Label lblStock = new();
    private Label lblValor = new();
    private Label lblEco = new();
    private Label lblSocial = new();

    private double ecoPromedio;
    private double socialPromedio;
    private int stockBajo;
    private int totalProductos;
    private decimal valorInventario;

    private float animationProgress = 0f;
    private System.Windows.Forms.Timer animationTimer = new();
    private ToolTip tooltip = new();

    private Rectangle ecoBarRect = Rectangle.Empty;
    private Rectangle socialBarRect = Rectangle.Empty;

    public DashboardForm(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;

        Text = "Dashboard Ejecutivo";
        Size = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.White;

        DoubleBuffered = true;

        BuildLayout();

        animationTimer.Interval = 15;
        animationTimer.Tick += AnimationTimer_Tick;

        Load += DashboardForm_Load;
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        animationProgress += 0.04f;

        if (animationProgress >= 1f)
        {
            animationProgress = 1f;
            animationTimer.Stop();
        }

        panelBars.Invalidate();
    }

    private void BuildLayout()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2
        };

        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(main);

        var kpiPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25),
            AutoScroll = false,
            WrapContents = false
        };

        main.Controls.Add(kpiPanel, 0, 0);

        lblTotal = CreateCard(kpiPanel, "📦 Productos");
        lblStock = CreateCard(kpiPanel, "⚠ Stock Bajo");
        lblValor = CreateCard(kpiPanel, "💰 Inventario");
        lblEco = CreateCard(kpiPanel, "🌱 EcoScore");
        lblSocial = CreateCard(kpiPanel, "🤝 SocialScore");

        var graphLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2
        };

        graphLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        graphLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        main.Controls.Add(graphLayout, 0, 1);

        panelBars = new ModernPanel { Dock = DockStyle.Fill, Margin = new Padding(15) };
        panelPie = new ModernPanel { Dock = DockStyle.Fill, Margin = new Padding(15) };

        panelBars.Paint += PanelBars_Paint;
        panelPie.Paint += PanelPie_Paint;
        panelBars.MouseMove += PanelBars_MouseMove;

        graphLayout.Controls.Add(panelBars, 0, 0);
        graphLayout.Controls.Add(panelPie, 1, 0);
    }

    private Label CreateCard(Control parent, string title)
    {
        var card = new ModernPanel
        {
            Width = 220,
            Height = 100,
            Margin = new Padding(15),
            BackColor = Color.FromArgb(40, 40, 40)
        };

        var value = new Label
        {
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 20),
            AutoSize = true
        };

        var label = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Location = new Point(20, 65),
            AutoSize = true
        };

        card.Controls.Add(value);
        card.Controls.Add(label);
        parent.Controls.Add(card);

        return value;
    }

    private async void DashboardForm_Load(object? sender, EventArgs e)
    {
        var data = await _dashboardService.GetMetricsAsync();

        totalProductos = data.TotalProductos;
        stockBajo = data.StockBajo;
        ecoPromedio = data.PromedioEco;
        socialPromedio = data.PromedioSocial;
        valorInventario = data.ValorInventario;

        lblTotal.Text = totalProductos.ToString();
        lblStock.Text = stockBajo.ToString();
        lblValor.Text = $"${valorInventario:N0}";
        lblEco.Text = ecoPromedio.ToString("F1");
        lblSocial.Text = socialPromedio.ToString("F1");

        animationProgress = 0f;
        animationTimer.Start();

        panelPie.Invalidate();
    }

    private void PanelBars_Paint(object? sender, PaintEventArgs e)
    {
        if (panelBars.Width <= 0 || panelBars.Height <= 0)
            return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var backBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
        g.FillRectangle(backBrush, e.ClipRectangle);

        int width = panelBars.Width;
        int height = panelBars.Height;

        int paddingLeft = 70;
        int paddingBottom = 60;
        int paddingTop = 30;

        int chartWidth = width - paddingLeft - 40;
        int chartHeight = height - paddingTop - paddingBottom;

        if (chartHeight <= 0)
            return;

        int baseY = paddingTop + chartHeight;

        // --- DIBUJAR GRID Y EJE Y ---
        using var gridPen = new Pen(Color.FromArgb(70, 70, 70), 1);
        using var axisPen = new Pen(Color.FromArgb(150, 150, 150), 1);
        using var font = new Font("Segoe UI", 9);

        for (int i = 0; i <= 5; i++)
        {
            float value = i * 20;
            int y = baseY - (int)(chartHeight * (value / 100));

            g.DrawLine(gridPen, paddingLeft, y, width - 20, y);

            string label = value.ToString();
            var size = g.MeasureString(label, font);

            g.DrawString(
                label,
                font,
                Brushes.LightGray,
                paddingLeft - size.Width - 8,
                y - size.Height / 2);
        }

        // Ejes
        g.DrawLine(axisPen, paddingLeft, paddingTop, paddingLeft, baseY);
        g.DrawLine(axisPen, paddingLeft, baseY, width - 20, baseY);

        // --- CALCULAR BARRAS ---
        int barWidth = 80;

        int ecoHeight = (int)(ecoPromedio / 100 * chartHeight * animationProgress);
        int socialHeight = (int)(socialPromedio / 100 * chartHeight * animationProgress);

        int ecoX = paddingLeft + chartWidth / 3 - barWidth / 2;
        int socialX = paddingLeft + chartWidth * 2 / 3 - barWidth / 2;

        ecoBarRect = ecoHeight > 0
            ? new Rectangle(ecoX, baseY - ecoHeight, barWidth, ecoHeight)
            : Rectangle.Empty;

        socialBarRect = socialHeight > 0
            ? new Rectangle(socialX, baseY - socialHeight, barWidth, socialHeight)
            : Rectangle.Empty;

        Color ecoColor =
            ecoPromedio >= 70 ? Color.FromArgb(76, 175, 80) :
            ecoPromedio >= 40 ? Color.FromArgb(255, 193, 7) :
            Color.FromArgb(244, 67, 54);

        if (!ecoBarRect.IsEmpty)
        {
            using var ecoBrush = new LinearGradientBrush(
                ecoBarRect,
                ecoColor,
                ControlPaint.Dark(ecoColor),
                LinearGradientMode.Vertical);

            g.FillRectangle(ecoBrush, ecoBarRect);
        }

        if (!socialBarRect.IsEmpty)
        {
            using var socialBrush = new LinearGradientBrush(
                socialBarRect,
                Color.FromArgb(33, 150, 243),
                Color.FromArgb(21, 101, 192),
                LinearGradientMode.Vertical);

            g.FillRectangle(socialBrush, socialBarRect);
        }

        using var labelFont = new Font("Segoe UI", 10, FontStyle.Bold);

        // Etiquetas eje X
        g.DrawString("EcoScore", labelFont, Brushes.White, ecoX - 10, baseY + 8);
        g.DrawString("SocialScore", labelFont, Brushes.White, socialX - 15, baseY + 8);

        // Valores arriba de barras
        if (!ecoBarRect.IsEmpty)
            g.DrawString($"{ecoPromedio:F1}", labelFont, Brushes.White, ecoX + 10, ecoBarRect.Y - 25);

        if (!socialBarRect.IsEmpty)
            g.DrawString($"{socialPromedio:F1}", labelFont, Brushes.White, socialX + 10, socialBarRect.Y - 25);
    }

    private void PanelBars_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!ecoBarRect.IsEmpty && ecoBarRect.Contains(e.Location))
            tooltip.SetToolTip(panelBars, $"EcoScore promedio: {ecoPromedio:F1}");
        else if (!socialBarRect.IsEmpty && socialBarRect.Contains(e.Location))
            tooltip.SetToolTip(panelBars, $"SocialScore promedio: {socialPromedio:F1}");
        else
            tooltip.SetToolTip(panelBars, "");
    }

    private void PanelPie_Paint(object? sender, PaintEventArgs e)
    {
        if (totalProductos == 0)
            return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var backBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
        g.FillRectangle(backBrush, e.ClipRectangle);

        int width = panelPie.Width;
        int height = panelPie.Height;

        int size = (int)(Math.Min(width, height) * 0.65);

        int x = (width - size) / 2;
        int y = (height - size) / 2 - 20;

        Rectangle rect = new Rectangle(x, y, size, size);

        float porcentajeBajo = (float)stockBajo / totalProductos;
        float angle = porcentajeBajo * 360;

        int stockNormal = totalProductos - stockBajo;

        // ----- SOMBRA -----
        Rectangle shadow = new Rectangle(x + 6, y + 6, size, size);

        using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
        {
            g.FillEllipse(shadowBrush, shadow);
        }

        // ----- COLORES -----
        Color colorNormal = Color.FromArgb(76, 175, 80);
        Color colorBajo = Color.FromArgb(244, 67, 54);

        using var greenBrush = new SolidBrush(colorNormal);
        using var redBrush = new SolidBrush(colorBajo);

        g.FillPie(redBrush, rect, 0, angle);
        g.FillPie(greenBrush, rect, angle, 360 - angle);

        // ----- DONUT HOLE -----
        int hole = size / 2;

        g.FillEllipse(
            new SolidBrush(Color.FromArgb(40, 40, 40)),
            x + size / 4,
            y + size / 4,
            hole,
            hole);

        // ----- TEXTO CENTRAL -----
        using var centerFont = new Font("Segoe UI", 16, FontStyle.Bold);

        string centerText = $"{porcentajeBajo:P0}";
        var centerSize = g.MeasureString(centerText, centerFont);

        g.DrawString(
            centerText,
            centerFont,
            Brushes.White,
            width / 2 - centerSize.Width / 2,
            height / 2 - centerSize.Height / 2);

        // ----- TEXTO ABAJO -----
        using var font = new Font("Segoe UI", 11, FontStyle.Bold);

        string label = $"{stockBajo} de {totalProductos} productos con Stock Bajo";

        var labelSize = g.MeasureString(label, font);

        g.DrawString(
            label,
            font,
            Brushes.White,
            width / 2 - labelSize.Width / 2,
            y + size + 15);

        // ----- LEYENDA -----
        int legendY = y + size + 45;

        using var legendFont = new Font("Segoe UI", 9);

        // Verde
        g.FillRectangle(new SolidBrush(colorNormal), width / 2 - 120, legendY, 14, 14);
        g.DrawString($"Stock Normal ({stockNormal})", legendFont, Brushes.LightGray, width / 2 - 100, legendY - 2);

        // Rojo
        g.FillRectangle(new SolidBrush(colorBajo), width / 2 + 40, legendY, 14, 14);
        g.DrawString($"Stock Bajo ({stockBajo})", legendFont, Brushes.LightGray, width / 2 + 60, legendY - 2);
    }
}