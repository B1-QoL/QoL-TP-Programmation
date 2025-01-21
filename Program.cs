using System.Diagnostics;
using static B1.Affichage;
    
namespace B1.ArchiBuilder;

public partial class ArchiBuilder 
{
    public static int Main(string[] args)
    {
        /*if(args.Length != 1) { // L'appel ne doit contenir que le lien du site.
            // Message d'erreur
            return 1; // Erreurs sur les paramètres d'appel.
        }

        if(!args[0].StartsWith($"https://intra.forge.ep{"i"}ta.fr/ep{"i"}ta-prepa-computer-science/")) 
        {
            // Message d'erreur
            return 2; // Lien invalide.
        }*/

        //string pageCode = GetWebsiteCode(args[0]);

       // string test = "<!DOCTYPE html><html lang=en><head><title>TinyTigrix</title><!--md5sum=b5c15f7c7966af149a8998d7cb780d8f-->";

        string pageCode = File.ReadAllText("/home/prenom.nom/Downloads/pageCode.html");

        //throw new NotImplementedException();
        
        Dictionary<string, List<string>> balises = ParsePage(pageCode);

        throw new NotImplementedException();
        
        /*if (!balises.ContainsKey("repoLink")) 
        {
            // Message d'erreur
            return 3; // Pas de lien de dépos git reconnu.
        }*/

        if (!balises.ContainsKey("creationCommands")) 
        {
            // Message d'erreur
            return 4; // Aucune commande de création de projet reconnue.
        }
        
        if (!balises.ContainsKey("tree")) 
        { 
            // Message d'erreur
            return 5; // Aucune arborescence de fichiers détectée.
        }

        /*RunCommandWithBash($"git clone {balises["repoLink"][0]}"); // Traiter le cas problème

        foreach(string command in shellCommands)
        {
            RunCommandWithBash(command); // Traiter le cas problème
        }*/
        
        (List<string> unknownFiles, List<string> alreadyFoundFiles) = BuildTree(balises["tree"]);

        if (alreadyFoundFiles.Count != 0)
        {
            Console.WriteLine("Certains fichiers existent déjà. Souhaitez-vous les modifier ? (o/n)");
            
            foreach (string file in alreadyFoundFiles)
            {
                Console.Write($"{file} : ");

                if (Console.ReadKey().KeyChar != 'o')
                    throw new NotImplementedException();
                    // Remove le fichier de la liste des fichiers à modifier
                    
                Console.WriteLine();
            }
        }

        if(unknownFiles.Count != 0)
        {
            Console.WriteLine("Certains fichiers n'ont pas été reconnus et ont été ignorés :");
            
            foreach(string file in unknownFiles)
            {
                Console.WriteLine($"\t· {file}");
            }
            
            Console.WriteLine();
            return 1;
        }


         // Console.WriteLine(RunCommandWithBash("git clone thomas.bobee@git.forge.epita.fr:p/epita-prepa-computer-science/prog-102-p-04-2029/epita-prepa-computer-science-prog-102-p-04-2029-thomas.bobee.git"));  // creates the sln 
         
         return 0;
    }
    
    // Traiter le cas où demande de mot de passe.
    private static string GetWebsiteCode(string url) => throw new NotImplementedException("");
    
    /// <summary>
    /// Execute une commande avec /bin/sh
    /// </summary>
    /// <param name="command">la commade a executer</param>
    /// <returns>l'output de la commande</returns>
    private static string RunCommandWithBash(string command)
    {
        var psi = new ProcessStartInfo();
        psi.FileName = "/bin/sh";
        psi.Arguments = $"-c \"{command}\"";
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        using var process = Process.Start(psi);

        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();

        return output;
    }
}
