using BCrypt.Net;

class Program
{
    static void Main()
    {
        Console.Write("Ingrese contraseña: ");
        string password = Console.ReadLine() ?? "";

        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        Console.WriteLine("\nHash BCrypt generado:");
        Console.WriteLine(hash);

        Console.ReadLine();
    }
}