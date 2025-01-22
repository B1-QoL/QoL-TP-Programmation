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
        //pageCode = RemoveComments(pageCode);
        //pageCode = RemoveTagContent(pageCode, "script");
        //pageCode = RemoveTagContent(pageCode,"style");
        List<string> parsedPage = ParseWithCode(KeepTagContent(pageCode, "code"));
        
        Print(parsedPage.Count);
        
        StreamWriter sw2 = new StreamWriter("test");
        sw2.Write(pageCode);
        sw2.Close();
        
        //throw new NotImplementedException();

        Dictionary<string, List<string>> balises = new();
        
        foreach (string line in parsedPage)
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
                var parsedLine = RemoveBefore(line,line.First(c => c is '\u251c' or '\u2500').ToString());
                
                if (!balises.TryAdd("tree", new List<string> {parsedLine}))
                    balises["tree"].Add(parsedLine);
            }
            else if (line.Contains("private") || line.Contains("public"))
            {
                break;
                if (line.Contains("string"))
                {
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

    private static string KeepTagContent(string htmlCode, string tag)
    {
        int j = -1, l2 = tag.Length, resLength = 0;
        StringBuilder res = new StringBuilder();
        bool inBeacon = false;
        
        foreach(char c in htmlCode)
        {
            if (!inBeacon)
            {
                if (j < l2 && ((j == -1 && c == '<') || (j != -1 && tag[j] == c)))
                {
                    res.Append(c);
                    ++resLength;
                    ++j;
                }
                else if (j == l2)
                {
                    inBeacon = true;
                    res.Append(c);
                    ++resLength;
                    j = -2;
                }
                else if(j != -1)
                {
                    res.Remove(resLength - j - 1, j + 1);
                    resLength -= j + 1;
                    j = -1;
                }
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
                
                res.Append(c);
                ++resLength;
            }
        }
        
        return res.ToString();
    }
    
    private static string RemoveComments(string htmlCode)
    {
        Stack<char> depth = new Stack<char>();
        StringBuilder res = new StringBuilder();
        char precedentChar = '\0';
        int resLength = 0;
        
        foreach(char c in htmlCode)
        {
            switch (c)
            {
                case '/' when depth.Count == 0 && precedentChar == '/' :
                    depth.Push('/');
                    res.Remove(resLength - 1, 1);
                    --resLength;
                    break;
                case '\n' when depth.Count != 0 && depth.Peek() == '/':
                    depth.Pop();
                    break;
                case '*' when depth.Count == 0 && precedentChar == '/' :
                    depth.Push('*');
                    res.Remove(resLength - 1, 1);
                    --resLength;
                    break;
                case '/' when depth.Count != 0 && precedentChar == '*' && depth.Peek() == '*':
                    depth.Pop();
                    break;
                default:
                {
                    if (depth.Count == 0)
                    {
                        res.Append(c);
                        ++resLength;
                    }
                    break;
                }
            }

            precedentChar = c;
        }

        return res.ToString(); 
    }
    
    private static List<string> ParseWithCode(string htmlCode)
    {
        List<string> res = new List<string>();
        string temp = "";
        int checkValidity = 0;
        
        foreach(char c in htmlCode)
        {
            checkValidity = c switch
            {
                '<' when checkValidity == 0 => 1,
                '/' when checkValidity == 1 => 2,
                'c' when checkValidity == 2 => 3,
                'o' when checkValidity == 3 => 4,
                'd' when checkValidity == 4 => 5,
                'e' when checkValidity == 5 => 6,
                '>' when checkValidity == 6 => 7,
                _ => 0
            };

            temp += c;
            
            if (temp.Length >= 7 && temp[^7..] == "</code>")
            {
                res.Add(temp);
                temp = "";
            }
            
        }

        return res;
    }
}
