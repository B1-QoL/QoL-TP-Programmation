using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using static B1.Affichage;

namespace B1.ArchiBuilder;

public partial class ArchiBuilder
{
    private static string GetSubject2(string subjectLink)
    {
        string cookiesFile = ".cookie-jar.ArchiBuilder.txt";

        ProcessStartInfo getCookies = CreateProcess($"curl -L -c {cookiesFile} https://cri.epita.fr/auth/login/?spnego=0");
        using Process? getCookiesProcess = Process.Start(getCookies);
        getCookiesProcess?.WaitForExit();
        string formToken = FindFormToken(getCookiesProcess.StandardOutput.ReadToEnd());
        
        ProcessStartInfo sendForm = CreateProcess($"curl -L -b {cookiesFile} -d \"usersame={AskUsername()}\" -d \"password={AskPassword()}\" -d \"csrfmiddlewaretoken={formToken}\" -e https://cri.epita.fr/auth/login/?spnego=0 https://cri.epita.fr/auth/login/");
        using Process? sendFormProcess = Process.Start(getCookies);
        sendFormProcess?.WaitForExit();
        Print(sendFormProcess.StandardOutput.ReadToEnd());
        Print("-------------------------------------");
        Print(sendFormProcess.StandardError.ReadToEnd());
        
        throw new NotImplementedException();
    }
    
    private static string GetSubject(string subjectLink)
    {
        string cookiesFile = ".cookie-jar.ArchiBuilder.txt";

        // Requete 1 : recuperer les cookies de session du cri
        ProcessStartInfo getTokensProcessStartInfo = CreateProcess($"curl -L --verbose -c {cookiesFile} {subjectLink}");

        using Process? getTokensProcess = Process.Start(getTokensProcessStartInfo);
        getTokensProcess?.WaitForExit();
        string criConnexionPageCode = getTokensProcess!.StandardOutput.ReadToEnd();
        
        string formToken = FindFormToken(criConnexionPageCode);
        
        string? redirectedLink = BeaconParse(getTokensProcess.StandardError.ReadToEnd(), "< location: ", "\n", false).Find(str => str.StartsWith("/auth/login"));
        redirectedLink = redirectedLink ?? throw new Exception();
        
        // Requete 2 : Connection au cri
        Print(File.ReadAllText(cookiesFile));
        Print(formToken);
        Console.WriteLine("Veuillez entrer vos identifiants Forge :");
        ProcessStartInfo getTPCodeProcessStartInfo = CreateProcess($"curl -L -b {cookiesFile} -c {cookiesFile} -d \"usersame={AskUsername()}\" -d \"password={AskPassword()}\" -d \"csrfmiddlewaretoken={formToken}\" -e {subjectLink} {"https://cri.epita.fr" + "/auth/login/"/*redirectedLink*/}");

        using Process? getTPCodeProcess = Process.Start(getTPCodeProcessStartInfo);
        getTPCodeProcess?.WaitForExit();
        string TPPageCode = getTPCodeProcess!.StandardOutput.ReadToEnd();

        Print(getTPCodeProcess.StandardError.ReadToEnd());
        Print(TPPageCode);
        
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
