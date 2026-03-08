using MiApp.Business;
using MiApp.Data;
using MiApp.Domain;

namespace MiApp.UI;

public class LoginForm : Form
{
    private TextBox txtUser = new();
    private TextBox txtPassword = new();
    private Button btnLogin = new();

    public User? AuthenticatedUser { get; private set; }

    public LoginForm()
    {
        this.Text = "Login";
        this.Size = new Size(300, 200);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        txtUser.PlaceholderText = "Usuario";
        txtUser.SetBounds(30, 30, 220, 25);

        txtPassword.PlaceholderText = "Contraseña";
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.SetBounds(30, 70, 220, 25);

        btnLogin.Text = "Ingresar";
        btnLogin.SetBounds(30, 110, 220, 30);
        btnLogin.Click += BtnLogin_Click;

        Controls.AddRange(new Control[] 
        { 
            txtUser, 
            txtPassword, 
            btnLogin 
        });
    }

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        string username = txtUser.Text.Trim();
        string password = txtPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Ingrese usuario y contraseña");
            return;
        }

        try
        {
            var db = new DatabaseService();

            // 🔎 1️⃣ Verificar si el usuario existe
            var userFromDb = await db.GetUserByUsernameAsync(username);

            if (userFromDb == null)
            {
                MessageBox.Show("Usuario NO encontrado en BD");
                return;
            }

            // 🔎 2️⃣ Mostrar hash almacenado y hash generado
            string generatedHash = PasswordHelper.Hash(password);

            // 🔎 3️⃣ Intentar login real
            var service = new AuthService(db);
            var user = await service.LoginAsync(username, password);

            if (user == null)
            {
                MessageBox.Show("Credenciales inválidas");
                return;
            }

            AuthenticatedUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Error real:\n\n" + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}