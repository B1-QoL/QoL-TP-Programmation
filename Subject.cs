using System.Net.Sockets;
using System.Text;

namespace B1.ArchiBuilder;

public class CriUser
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
        Console.WriteLine("Enter your Forge credentials!");
        var username = AskUsername();
        var password = AskPassword();

        return new CriUser(username, password);
    }

    private static string AskUsername()
    {
        Console.Write("Username: ");
        string? input = Console.ReadLine();
        if (input == null) throw new ArgumentException("Forge username should not be empty");
        return input;
    }

    private static string AskPassword()
    {
        Console.Write("Password: ");
        var input = new StringBuilder();

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

        if (input.Length == 0) throw new ArgumentException("Forge password should not be empty");

        return input.ToString();
    }
}
public static partial class ArchiBuilder
{
    private static string GetSubject(string subjectLink, CriUser user)
    {
        using var handler = new HttpClientHandler();
        handler.AllowAutoRedirect = true;
        using var client = new HttpClient(handler);

        // Get session cookies and `csrfmiddlewaretoken`
        var initialReq = new HttpRequestMessage(HttpMethod.Get, new Uri(subjectLink));
        var initialRes = client.Send(initialReq);
        var initialContent = new StreamReader(initialRes.Content.ReadAsStream()).ReadToEnd();
        var formToken = FindFormToken(initialContent);

        // Login with credentials
        var content = new FormUrlEncodedContent(new[]
        {
            KeyValuePair.Create("csrfmiddlewaretoken", formToken),
            KeyValuePair.Create("username", user.Username),
            KeyValuePair.Create("password", user.Password)
        });

        var loginReq = new HttpRequestMessage(HttpMethod.Post, initialRes.RequestMessage?.RequestUri);
        loginReq.Headers.Referrer = initialRes.RequestMessage?.RequestUri;
        loginReq.Content = content;
        var subjectResponse = client.Send(loginReq);
        if (!subjectResponse.IsSuccessStatusCode) throw new IOException("Could not correctly authenticate or fetch the subject");
        var subjectContent = new StreamReader(subjectResponse.Content.ReadAsStream()).ReadToEnd();

        return subjectContent;
    }
}
