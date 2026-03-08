using MiApp.Business;

namespace MiApp.UI;

public class AuditForm : Form
{
    private readonly AuditService _auditService;

    private DataGridView dgv = new();
    private Button btnExport = new();

    public AuditForm(AuditService auditService)
    {
        _auditService = auditService;

        this.Text = "Auditoría del Sistema";
        this.Size = new Size(900, 550);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        ConfigurarGrid();
        ConfigurarBoton();

        this.Controls.Add(dgv);
        this.Controls.Add(btnExport);

        this.Load += AuditForm_Load;
    }

    private void ConfigurarGrid()
    {
        dgv.Dock = DockStyle.Fill;
        dgv.ReadOnly = true;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.AllowUserToAddRows = false;
        dgv.RowHeadersVisible = false;
        dgv.BackgroundColor = Color.White;

        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
    }

    private void ConfigurarBoton()
    {
        btnExport.Text = "Exportar CSV";
        btnExport.Dock = DockStyle.Bottom;
        btnExport.Height = 45;
        btnExport.BackColor = Color.FromArgb(0, 120, 215);
        btnExport.ForeColor = Color.White;
        btnExport.FlatStyle = FlatStyle.Flat;
        btnExport.FlatAppearance.BorderSize = 0;

        btnExport.Click += async (s, e) => await BtnExport_ClickAsync();
    }

    private async void AuditForm_Load(object? sender, EventArgs e)
    {
        await CargarAuditoriaAsync();
    }

    private async Task CargarAuditoriaAsync()
    {
        var logs = await _auditService.GetAllAsync();

        dgv.DataSource = logs.Select(x => new
        {
            Usuario = x.Username,
            Acción = x.Action,
            Descripción = x.Description,
            Fecha = x.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList();
    }

    private async Task BtnExport_ClickAsync()
    {
        var logs = await _auditService.GetAllAsync();

        var exporter = new CsvExportService();
        var csv = exporter.ExportAudit(logs.ToList());

        using SaveFileDialog sfd = new();
        sfd.Filter = "CSV files (*.csv)|*.csv";
        sfd.FileName = "auditoria.csv";

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            await File.WriteAllTextAsync(sfd.FileName, csv);

            MessageBox.Show(
                "Auditoría exportada correctamente.",
                "Exportación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}