namespace MiApp.Domain;

public enum UserRole
{
    Admin = 1,
    Operador = 2
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}