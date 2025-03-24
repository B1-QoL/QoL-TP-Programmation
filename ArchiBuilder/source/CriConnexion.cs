using System.Net.Sockets;
using System.Text;

namespace ArchiBuilder;

internal class CriUser
{
    public string Username { get; }
    public string Password { get; }

    private CriUser(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public static CriUser AskCredentials()
    {
        Console.WriteLine("Veuillez entrer vous identifiants Forge :");

        string username = AskLogin();
        string password = AskPassword();

        return new CriUser(username, password);
    }

    private static string AskLogin()
    {
        Console.Write("Login: ");
        string? input = Console.ReadLine();
        if (input == null) throw new ArchiBuilderExceptions.EmptyLoginException();
        return input;
    }

    private static string AskPassword()
    {
        Console.Write("Password: ");
        StringBuilder input = new StringBuilder();

        while (Console.ReadKey(true) is {} info && info.Key != ConsoleKey.Enter)
        {
            (int x, int y) = (Console.CursorLeft, Console.CursorTop);

            if (info.Key == ConsoleKey.Backspace)
            {
                if (input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.SetCursorPosition(x - 1, y);
                    Console.Write(" ");
                    Console.SetCursorPosition(x - 1, y);
                }
            }
            else
            {
                input.Append(info.KeyChar);
                Console.Write("*");
            }
        }

        Console.WriteLine();

        if (input.Length == 0) throw new ArchiBuilderExceptions.EmptyPasswordException();

        return input.ToString();
    }
}