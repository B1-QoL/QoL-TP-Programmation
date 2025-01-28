using System.Diagnostics;
using static B1.Affichage;

namespace B1.ArchiBuilder;

public partial class ArchiBuilder
{
    private static string GetSubject(string subjectLink)
    {
        string cookiesFile = ".cookie-jar.ArchiBuilder.txt";

        // Requete 1 : recuperer les cookies de session du cri
        ProcessStartInfo getTokensProcessStartInfo = CreateProcess($"\"curl -L --verbose -c {cookiesFile} {subjectLink}\"");

        using Process? getTokensProcess = Process.Start(getTokensProcessStartInfo);
        getTokensProcess?.WaitForExit();
        string criConnexionPageCode = getTokensProcess!.StandardOutput.ReadToEnd();
        
        string formToken = FindFormToken(criConnexionPageCode);

        Print(criConnexionPageCode);
        var redirectedLink = BeaconParse(criConnexionPageCode, "location:", "<",false);
        Print(redirectedLink);
        
        // Requete 2 : Connection au cri
        ProcessStartInfo getTPCodeProcessStartInfo = CreateProcess($"\"curl -L -b {cookiesFile} -c {cookiesFile} -d \"usersame={AskUsername()}\" -d \"password={AskPassword()}\" -d \"csrfmiddlewaretoken={formToken}\" {subjectLink} \"");

        using Process? getTPCodeProcess = Process.Start(getTPCodeProcessStartInfo);
        getTPCodeProcess?.WaitForExit();
        string TPPageCode = getTPCodeProcess!.StandardOutput.ReadToEnd();
        string formToken2 = FindFormToken(TPPageCode);
        //Print(formToken2);
        
        return "";
    }

    private static ProcessStartInfo CreateProcess(string command) =>
        new()
        {
            #if LINUX
                Arguments = $"-c {command}",
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

    private static string AskUsername() => throw new NotImplementedException();
    
    private static string AskPassword() => throw new NotImplementedException();
}