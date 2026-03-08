using BCrypt.Net;

namespace MiApp.Business;

public static class PasswordHelper
{
    // Genera hash seguro con salt automático
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verifica contraseña contra hash almacenado
    public static bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}