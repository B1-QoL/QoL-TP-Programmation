namespace B1.ArchiBuilder;

public class ArchiBuilder 
{
    public static int Main(string[] args)
    {
        if(args.Length != 1) // L'appel ne doit contenir que le lien du site.
        {
            // Message d'erreur
            return 1;
        }

        if(!args[0].StartWith(@"https://intra.forge.epita.fr/epita-prepa-computer-science/")
        {
            // Message d'erreur
            return 2;
        }

        string pageCode = GetWebsiteCode(args[0]);
        Dictionnary<string, string[]> balises = ParsePage(pageCode);
        
        // BuildTree("epita-prepa-computer-science-prog-102-p-04-2029-firstname.lastname\n\u251c\u2500\u2500 ObelixAndCo\n\u2502   \u251c\u2500\u2500 Cells\n\u2502   \u2502   \u251c\u2500\u2500 Cell.cs\n\u2502   \u2502   \u251c\u2500\u2500 Forest.cs\n\u2502   \u2502   \u251c\u2500\u2500 Hut.cs\n\u2502   \u2502   \u251c\u2500\u2500 Pond.cs\n\u2502   \u2502   \u2514\u2500\u2500 Quarry.cs\n\u2502   \u251c\u2500\u2500 People\n\u2502   \u2502   \u251c\u2500\u2500 Fisher.cs\n\u2502   \u2502   \u251c\u2500\u2500 Hunter.cs\n\u2502   \u2502   \u251c\u2500\u2500 Person.cs\n\u2502   \u2502   \u2514\u2500\u2500 Sculptor.cs\n\u2502   \u251c\u2500\u2500 Grid.cs\n\u2502   \u251c\u2500\u2500 IoManager.cs\n\u2502   \u251c\u2500\u2500 ObelixAndCo.csproj\n\u2502   \u251c\u2500\u2500 Program.cs\n\u2502   \u251c\u2500\u2500 RandomPrice.cs\n\u2502   \u2514\u2500\u2500 Utils.cs\n\u251c\u2500\u2500 Tests\n\u2502   \u2514\u2500\u2500 Insert your Tests files\n\u251c\u2500\u2500 .gitignore\n\u251c\u2500\u2500 ObelixAndCo.sln\n\u2514\u2500\u2500 README\n");
        return 0;
    }

    private readonly HttpClient _client = new();
    
    // Traiter le cas où demande de mot de passe.
    private static string GetWebsiteCode(string url) 
    {
        return await _client.GetStringAsync(url);
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
    private static (List<string>, bool) BuildTree(string tree) 
    {
        List<string> unknownFiles = new List<string>();
        bool fileAlreadyExists = false;
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
                        if (File.Exists(name))
                            fileAlreadyExists = true;
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

                    if (File.Exists(name))
                        fileAlreadyExists = true;
                    else
                        File.Create(path + name);
                }
                else
                    unknownFiles.Add(name);
            }
        }
        
        return (unknownFiles, fileAlreadyExists);
    }

    private static Dictionnary<string, string[]> ParsePage(string pageCode)
    {
        Dictionnary<string, string[]> balises = new();
        string[] pageArray = pageCode.Split('\n');
        
        foreach (string line in pageArray)
        {
            if(line.Contains("@git.forge.epita.fr"))
            {
                string[] gitRepoLine = line.Split('"');
                foreach(string part in gitRepoLine)
                {
                    if(part.Contains("@git.forge.epita.fr"))
                        balises.Add("repoLink", new string[](part))
                }
            }
        }

        return balises;
    }
        
    private static void WriteFiles()
    {
        
    }
}
