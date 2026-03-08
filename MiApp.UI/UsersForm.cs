using MiApp.Data;
using MiApp.Domain;
using MiApp.Business;

namespace MiApp.UI;

public class UsersForm : Form
{
    private DataGridView dgv = new();
    private Button btnCreate = new();
    private Button btnToggle = new();

    public UsersForm()
    {
        this.Text = "Gestión de Usuarios";
        this.Size = new Size(700, 400);
        this.StartPosition = FormStartPosition.CenterParent;

        dgv.Dock = DockStyle.Top;
        dgv.Height = 250;
        dgv.ReadOnly = true;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        btnCreate.Text = "Crear Usuario";
        btnCreate.Top = 270;
        btnCreate.Left = 20;
        btnCreate.Click += BtnCreate_Click;

        btnToggle.Text = "Activar / Desactivar";
        btnToggle.Top = 270;
        btnToggle.Left = 250;
        btnToggle.Width = 180;  
        btnToggle.Height = 30;
        btnToggle.Click += BtnToggle_Click;
        this.Controls.AddRange(new Control[] { dgv, btnCreate, btnToggle });

        this.Load += UsersForm_Load;
    }

    private async void UsersForm_Load(object? sender, EventArgs e)
    {
        var db = new DatabaseService();
        dgv.DataSource = await db.GetUsersAsync();
    }

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        string username = Microsoft.VisualBasic.Interaction.InputBox("Username:");
        string password = Microsoft.VisualBasic.Interaction.InputBox("Password:");

        var db = new DatabaseService();
        string hash = PasswordHelper.Hash(password);
        await db.CreateUserAsync(username, hash, UserRole.Operador);

        dgv.DataSource = await db.GetUsersAsync();
    }

    private async void BtnToggle_Click(object? sender, EventArgs e)
    {
        if (dgv.CurrentRow?.DataBoundItem is not User user)
            return;

        var db = new DatabaseService();
        await db.ToggleUserActiveAsync(user.Id, !user.IsActive);

        dgv.DataSource = await db.GetUsersAsync();
    }
}