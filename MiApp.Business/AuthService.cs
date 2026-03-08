using MiApp.Data;
using MiApp.Domain;

namespace MiApp.Business;

public class AuthService
{
    private readonly DatabaseService _db;

    public AuthService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _db.GetUserByUsernameAsync(username);

        // Usuario no existe o está inactivo
        if (user == null || !user.IsActive)
        {
            await _db.InsertAuditAsync(new AuditLog
            {
                UserId = 0,
                Action = "LOGIN_FAILED",
                Description = $"Intento fallido para usuario: {username}",
                CreatedAt = DateTime.UtcNow
            });

            return null;
        }

        // 🔐 Verificación correcta con BCrypt
        if (!PasswordHelper.Verify(password, user.PasswordHash))
        {
            await _db.InsertAuditAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "LOGIN_FAILED",
                Description = "Contraseña incorrecta",
                CreatedAt = DateTime.UtcNow
            });

            return null;
        }

        // ✅ Login exitoso
        await _db.InsertAuditAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "LOGIN_SUCCESS",
            Description = "Inicio de sesión exitoso",
            CreatedAt = DateTime.UtcNow
        });

        return user;
    }
}