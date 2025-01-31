using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using static B1.Affichage;

namespace B1.ArchiBuilder;

public partial class ArchiBuilder
{    
    private static string GetSubject(string subjectLink)
    {
        string cookiesFile = ".cookie-jar.ArchiBuilder.txt";
        try
        {
            File.Create(cookiesFile);
        }
        catch(Exception)
        {
            // L'utilisateur n'a pas les droits pour écrire dans le dossier courant
            return "1";
        }

        // Requête 1 : Connection à la page de connexion du cri
        ProcessStartInfo getTokensProcessStartInfo = CreateProcess($"curl -L --verbose -c {cookiesFile} {subjectLink}");

        using Process? getTokensProcess = Process.Start(getTokensProcessStartInfo);
        getTokensProcess?.WaitForExit();
        string criConnexionPageCode = getTokensProcess!.StandardOutput.ReadToEnd();
        
        string formToken = FindFormToken(criConnexionPageCode);
        
        string? redirectedLink = BeaconParse(getTokensProcess.StandardError.ReadToEnd(), "< location: ", "\n", false).Find(str => str.StartsWith("/auth/login"));

        if(redirectedLink is null)
            // Le lien est invalide
            return "2";
        
        // Requête 2 : Authentification
        Console.WriteLine("Veuillez entrer vos identifiants Forge :");
        ProcessStartInfo getTPCodeProcessStartInfo = CreateProcess(
            $"curl -L " +
            $"-b {cookiesFile} " +
            $"-c {cookiesFile} " +
            $"-d 'usersame={AskUsername()}' " +
            $"-d 'password={AskPassword()}' " +
            $"-d 'csrfmiddlewaretoken={formToken}' " +
            $"-e {subjectLink} " +
            $"{redirectedLink} "
        );

        using Process? getTPCodeProcess = Process.Start(getTPCodeProcessStartInfo);
        getTPCodeProcess?.WaitForExit();
        string TPPageCode = getTPCodeProcess!.StandardOutput.ReadToEnd();

        Print(getTPCodeProcess.StandardError.ReadToEnd());
        Print(TPPageCode);
        File.Delete(cookiesFile);
        
        return "";
    }

    private static ProcessStartInfo CreateProcess(string command) =>
        new()
        {
            #if LINUX
                Arguments = $"-c \"{command}\"",
                FileName = "/bin/sh",
            #elif WINDOWS
            
            #endif
            
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

    private static string AskUsername()
    {
        Console.Write("Username : ");
        string input = Console.ReadLine()!;
        return input;
    }

    private static string AskPassword()
    {
        Console.Write("Password : ");
        StringBuilder input = new StringBuilder();
        ConsoleKeyInfo key = Console.ReadKey(true);
        
        while (key.Key is not ConsoleKey.Enter)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            
            if (key.Key == ConsoleKey.Backspace)
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
                input.Append(key.KeyChar);
                Console.Write("*");
            }
            
            key = Console.ReadKey(true);
        }
        
        Console.WriteLine();
        return input.ToString();
    }
}
