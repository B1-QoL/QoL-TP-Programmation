// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.XPath;

namespace B1.ArchiBuilder;

public class ArchiBuilder 
{
    public static int Main(string[] args)
    {
         if(args.Length != 1) { // L'appel ne doit contenir que le lien du site.
            // Message d'erreur
            return 1;
         }

         if(!args[0].StartsWith(@"https://intra.forge.epita.fr/epita-prepa-computer-science/")) // lien TP de prog epita
         {
             // Message d'erreur
             return 2;
         }

         string pageCode = GetWebsiteCode(args[0]);
         Dictionary<string, string[]> balises = ParsePage(pageCode);

         if (!balises.ContainsKey("tree")) { // Ne contient pas de tree reconnu
             // Message d'erreur
             return 3;
         }
         
         if (!balises.ContainsKey("repoLink")) { // Ne contient pas de lien repo reconnu
             // Message d'erreur
             return 4;
         }
         
         if (!balises.ContainsKey("shellCommands")) { // Ne contient pas de commandes de creation des sln et .csproj
             // Message d'erreur
             return 4;
         }
         
         var res = BuildTree(balises["tree"][0]);
         
         // var res = BuildTree("epita-prepa-computer-science-prog-102-p-04-2029-firstname.lastname\n\u251c\u2500\u2500 ObelixAndCo\n\u2502   \u251c\u2500\u2500 Cells\n\u2502   \u2502   \u251c\u2500\u2500 Cell.cs\n\u2502   \u2502   \u251c\u2500\u2500 Forest.cs\n\u2502   \u2502   \u251c\u2500\u2500 Hut.cs\n\u2502   \u2502   \u251c\u2500\u2500 Pond.cs\n\u2502   \u2502   \u2514\u2500\u2500 Quarry.cs\n\u2502   \u251c\u2500\u2500 People\n\u2502   \u2502   \u251c\u2500\u2500 Fisher.cs\n\u2502   \u2502   \u251c\u2500\u2500 Hunter.cs\n\u2502   \u2502   \u251c\u2500\u2500 Person.cs\n\u2502   \u2502   \u2514\u2500\u2500 Sculptor.cs\n\u2502   \u251c\u2500\u2500 Grid.cs\n\u2502   \u251c\u2500\u2500 IoManager.cs\n\u2502   \u251c\u2500\u2500 ObelixAndCo.csproj\n\u2502   \u251c\u2500\u2500 Program.cs\n\u2502   \u251c\u2500\u2500 RandomPrice.cs\n\u2502   \u2514\u2500\u2500 Utils.cs\n\u251c\u2500\u2500 Tests\n\u2502   \u2514\u2500\u2500 Insert your Tests files\n\u251c\u2500\u2500 .gitignore\n\u251c\u2500\u2500 ObelixAndCo.sln\n\u2514\u2500\u2500 README\n");
         
         if (res.alreadyFoundFiles.Count != 0) {
             Console.WriteLine("Couldn't create following files/directory because they were already found: ");
             foreach (var file in res.alreadyFoundFiles) {
                 Console.WriteLine("    ·" + file);
             }
         }
         
         if (res.unknownFiles.Count != 0) {
             Console.WriteLine("Couldn't create following files because they were of unknown extension: ");
             foreach (var file in res.unknownFiles) {
                 Console.WriteLine("    ·" + file);
             }
         }

         // Console.WriteLine(RunCommandWithBash("git clone thomas.bobee@git.forge.epita.fr:p/epita-prepa-computer-science/prog-102-p-04-2029/epita-prepa-computer-science-prog-102-p-04-2029-thomas.bobee.git"));  // creates the sln 
         
         // tout c'est bien passe (mettre un e accent aigu)
         return 0;
    }

    private static readonly HttpClient _client = new();
    
    // Traiter le cas où demande de mot de passe.
    private static string GetWebsiteCode(string url) {

        throw new NotImplementedException("");
        // return _client.GetStringAsync(url).Result;
        return "test";
    }

    /// <summary>
    /// Renvoie le nom du fichier ou dossier de la ligne dans l'arborescence. Plus précisément, c'est la fin de la chaîne de charactères <c>line</c> à partir de la première lettre trouvée qui est renvoyée.
    /// </summary>
    /// <param name="line">Ligne à traiter.</param>
    /// <returns>Nom de l'objet trouvé.</returns>
    private static string GetName(string line) 
    {
        string res = "";
        bool nameStarted = false;
        
        foreach (var c in line) 
        {
            if (nameStarted || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '.') {
                res += c;
                nameStarted = true;
            }
        }

        return res;
    }

    /// <summary>
    /// Renvoie la profondeur du fichier/dossier trouvé en fonction du nombre de "│" (\u2502).
    /// </summary>
    /// <param name="line">Ligne à traiter.</param>
    /// <returns>Profondeur de l'objet dans l'arbre.</returns>
    private static int GetDepth(string line) 
    {
        var res = 0;

        foreach (var c in line)
            if (c == '\u2502' || c == '\u251c' || c == '\u2514')
                res++;

        return res;
    }

    /// <summary>
    /// Crée l'arborescence de fichier conformément à celle donnée en paramètre.
    /// </summary>
    /// <param name="tree">L'arborescence à créer.</param>
    /// <returns>Liste contenant les objets non crées, tels que des dossiers vides par exemple.</returns>
    /// <remarks>
    /// Les fichiers cachés (commençant par ".") seront également créés. 
    /// </remarks>
    private static (List<string> unknownFiles, List<string> alreadyFoundFiles) BuildTree(string tree) 
    {
        List<string> unknownFiles = new List<string>();
        List<string> alreadyFoundFiles = new List<string>();
        // bool fileAlreadyExists = false;
        Stack<(string, int)> folderStack = new ();

        string[] treeArray = tree.Split('\n');
        
        foreach (string line in treeArray)
        {
            int depth = GetDepth(line);
            string name = GetName(line);

            if (depth == 0) // Ne pas créer de racine.
                continue;
            
            if (!name.Contains('.') && name != "README") // Vérifie que l'objet est un dossier.
            {
                name += "/";

                string path = "";
                bool isPlaced = false;
                
                while (folderStack.Count != 0 && !isPlaced)
                {
                    (string previousPath, int previousDepth) = folderStack.Peek();

                    if (previousDepth < depth)
                        (path, isPlaced) = (previousPath, true);
                    else
                        folderStack.Pop();
                }
                
                folderStack.Push((path + name, depth));
                
                (string previousPath2, _) = folderStack.Peek();
                string[] pathArray = previousPath2.Split('/');
                
                if (!pathArray.Contains("Tests"))
                    Directory.CreateDirectory(path + name);
            }
            else if (name.Contains('.')) // Fichier avec extension.
            {
                string extension = name.Split('.')[^1], path = "";
                bool isFilePlaced = false;

                while (folderStack.Count != 0 && !isFilePlaced)
                {
                    (string previousPath, int previousDepth) = folderStack.Peek();
                    
                    if (previousDepth < depth)
                        (path, isFilePlaced) = (previousPath, true);
                    else
                        folderStack.Pop();
                }

                switch (extension) 
                {
                    case "cs": case "gitignore":
                        if (File.Exists(path + name))
                            // fileAlreadyExists = true;
                            alreadyFoundFiles.Add(path + name);
                        else
                            File.Create(path + name);
                        break;
                    case "csproj" or "sln" :
                        break;
                    default :
                        unknownFiles.Add(name);
                        break;
                }
            }
            else 
            {
                if (name == "README") 
                {
                    string path = "";
                    bool isFilePlaced = false;

                    while (folderStack.Count != 0 && !isFilePlaced) 
                    {
                        (string previousPath, int previousDepth) = folderStack.Peek();
                        
                        if (previousDepth < depth)
                            (path, isFilePlaced) = (previousPath, true);
                        else 
                            folderStack.Pop();
                    }

                    if (File.Exists(path + name))
                        // fileAlreadyExists = true;
                        alreadyFoundFiles.Add(path + name);
                    else
                        File.Create(path + name);
                }
                else
                    unknownFiles.Add(name);
            }
        }
        
        return (unknownFiles, alreadyFoundFiles); // fileAlreadyExists);
    }

    /// <summary>
    /// retourne le dictionnaire avec le contenu requis pour faire l'arborescence
    /// </summary>
    /// <param name="pageCode">le code de la page</param>
    /// <returns>Un dictoinnaire avec les cle: "repoLink", "tree" et "shellCommands"</returns>
    private static Dictionary<string, string[]> ParsePage(string pageCode)
    {
        Dictionary<string, string[]> balises = new();
        string[] pageArray = pageCode.Split('\n');
        
        foreach (string line in pageArray)
        {
            if(line.Contains("@git.forge.epita.fr")) // repo
            {
                string[] gitRepoLine = line.Split('"');
                foreach(string part in gitRepoLine) {
                    if (part.Contains("@git.forge.epita.fr"))
                        balises.Add("repoLink", new string[]{part});
                }
            } 
            else if (line.Contains("<code") && line.Contains("epita-perpa-computer-science") && line.Contains("\u251c\u2500")) { // tree
                string[] treeLine = line.Split(">");
                foreach (var part in treeLine) {
                    if (part.Contains("epita-perpa-computer-science") && part.Contains("\u251c\u2500")) {
                        string[] newPart = part.Split("<");
                        string tree = newPart[0];
                        balises.Add("tree", new string[] { tree });
                    }
                }
            } else if (line.Contains("<code") && line.Contains("dotnet new sln")) { // commands
                string[] slnLine = line.Split(">")[3..];  // les trois premiers splits ne sont pas utiles
                string commands = ""; // les trois commandes a executer
                foreach (var part in slnLine) {
                    if (part.Contains("dotnet new sln"))
                        commands += part + "\n";
                    else if (part.Contains("dotnet new console"))
                        commands += part + "\n";
                    else if (part.Contains("dotnet sln add"))
                        commands += part + "\n";
                }
                
                balises.Add("shellCommands", new string[] { commands });
            }
        }

        return balises;
    }
        
    private static void WriteFiles(List<string> files)
    {
        
    }
    
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
