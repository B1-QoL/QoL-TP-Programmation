using System.Text;

namespace B1.ArchiBuilder;
using static B1.Affichage;
// Partie analysant le code de la page
public partial class ArchiBuilder
{
    private static List<(string, List<string>)> _filesToEdit = new ();
    
    /// <summary>
    /// retourne le dictionnaire avec le contenu requis pour faire l'arborescence
    /// </summary>
    /// <param name="pageCode">le code de la page</param>
    /// <returns>Un dictoinnaire avec les cle: "repoLink", "tree" et "shellCommands"</returns>
    private static Dictionary<string, List<string>> ParsePage(string pageCode)
    {
        Print(pageCode.Length);
        
        pageCode = RemoveTagContent(pageCode, "style");
        pageCode = RemoveTagContent(pageCode,"script");
        
        Print(pageCode.Length);

        throw new NotImplementedException();
        
        //pageCode = RemoveTagContent(pageCode, "script");
        Dictionary<string, List<string>> balises = new();
        string[] pageArray = pageCode.Split('\n');
        
        foreach (string line in pageArray)
        { 
            if (line.Contains($"@git.forge.ep{"i"}ta.fr")) // Lien d'un dépot
            {
                string[] gitRepoLine = line.Split('"');
                foreach (string part in gitRepoLine)
                {
                    if (part.Contains($"@git.forge.ep{"i"}ta.fr"))
                        balises.Add("repoLink", new List<string> { part });
                }
            }
            else if (line.Contains("dotnet new") || line.Contains("dotnet sln")) // Commandes de création du projet
            {
                string lineParsed = RemoveBefore(line, "dotnet");
                lineParsed = RemoveAfter(lineParsed, '<');
                string command = "";
                bool isInCommand = false;
                foreach (string part in lineParsed.Split(' '))
                {
                    if (part == "dotnet")
                    {
                        command += part + " ";
                        isInCommand = true;
                    }
                    else if (part.Contains('<'))
                        isInCommand = false;
                    else if (isInCommand)
                        command += part + " ";
                }

                if (!balises.TryAdd("creationCommands", new List<string> { command }))
                {
                    balises["creationCommands"].Add(command);
                }
                
            }
            else if (line.Contains("\u251c\u2500")) // Arborescence de fichiers
            {
                string parsedLine = RemoveTags(line);
                
                if (!balises.TryAdd("tree", new List<string> {parsedLine}))
                    balises["tree"].Add(parsedLine);
            }
            else if (line.Contains("private") || line.Contains("public"))
            {
                if (line.Contains("string"))
                {
                    Print(line);
                    string parsedLine = RemoveTags(line);
                    Print(parsedLine);
                    break;
                }
                
            }
        }

        return balises;
    }

    private static string RemoveBefore(string line, string pattern)
    {
        int i = 0, l1 = line.Length, j = 0, l2 = pattern.Length;
        while (i < l1)
        {
            if (j == l2)
                return line.Substring(i - j);
            if (line[i] == pattern[j])
                ++j;
            else if (j != 0)
                j = 0;
            ++i;
        }

        return i == l1 ? line : line.Substring(i - j);
    }
    
    private static string RemoveAfter(string line, char pattern)
    {
        int i = 0, l1 = line.Length;
        string res = "";
        while (i < l1 && line[i] != pattern)
        {
            res += line[i];
            ++i;
        }

        return res;
    }

    private static string RemoveTags(string line)
    {
        int i = 0, l = line.Length;
        Stack<char> depth = new Stack<char>();
        string res = "";
        while (i < l)
        {
            switch (line[i])
            {
                case '<':
                    depth.Push('<');
                    break;
                case '>' when depth.Count != 0:
                    depth.Pop();
                    break;
                default:
                {
                    if (depth.Count == 0)
                        res += line[i];
                    break;
                }
            }

            ++i;
        }

        return res;
    }

    private static string RemoveTagContent(string htmlCode, string tag)
    {
        int j = -1, l2 = tag.Length, resLength = 0;
        StringBuilder res = new StringBuilder();
        bool inBeacon = false;
        
        foreach(char c in htmlCode)
        {
            if (!inBeacon)
            {
                if (j < l2 && ((j == -1 && c == '<') || (j != -1 && tag[j] == c)))
                    ++j;
                else if (j == l2)
                {
                    inBeacon = true;
                    j = -2;
                    res.Remove(resLength - l2 - 1, l2 + 1);
                    resLength -= l2 + 1;
                    continue;
                }
                else
                    j = -1;

                res.Append(c);
                ++resLength;
            }
            else
            {
                if (j < l2 && ((j == -2 && c == '<') || (j == -1 && c == '/') || (j > -1 && tag[j] == c)))
                    ++j;
                else if (j == l2)
                {
                    inBeacon = false;
                    j = -1;
                }
                else
                    j = -2;
            }
        }
        
        return res.ToString();
    }

}
