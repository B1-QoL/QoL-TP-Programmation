namespace B1.ArchiBuilder;

// Partie construisant l'arborescence de fichiers
public static partial class ArchiBuilder
{
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
            if (nameStarted || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '.')
            {
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
    private static (List<string> unknownFiles, List<string> alreadyFoundFiles) BuildTree(List<string> tree)
    {
        List<string> unknownFiles = new List<string>();
        List<string> alreadyFoundFiles = new List<string>();
        Stack<(string, int)> folderStack = new();

        foreach (string line in tree)
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
                _filesToEdit.Add((name, new()));
                string extension = name.Split('.')[^1],
                    path = "";
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
                    case "cs":
                    case "gitignore":
                        if (File.Exists(path + name))
                            alreadyFoundFiles.Add(path + name);
                        else
                            File.Create(path + name);
                        break;
                    case "csproj" or "sln":
                        break;
                    default:
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
                        alreadyFoundFiles.Add(path + name);
                    else
                        File.Create(path + name);
                }
                else
                    unknownFiles.Add(name);
            }
        }

        return (unknownFiles, alreadyFoundFiles);
    }
}
