using System.Text;
using System.Web;

namespace ArchiBuilder;

// Partie analysant le code de la page
public partial class ArchiBuilder
{
    private static Subject ParseSubject(List<string> parsedPage)
    {
        Subject subject = new Subject();

        string? currentFile = null;
        
        for (int i = 0; i < parsedPage.Count - 1; i++)
        {
            string lineWithoutBeacons = HttpUtility.HtmlDecode(BeaconParse(parsedPage[i], ">", "</code>", false)[0]); // Retirer l'encodage HTML.
            if (lineWithoutBeacons.Contains("dotnet new") || lineWithoutBeacons.Contains("dotnet sln")) // Commandes de création du projet
            {
                string[] parsedLine = lineWithoutBeacons.Split('\n');

                foreach (string line in parsedLine)
                {
                    if (line.Contains("dotnet"))
                        subject.CreationCommands.Add(line);
                }
            }
            else if (lineWithoutBeacons.Contains("\u251c\u2500")) // Arborescence de fichiers
                ListFoldersAndFiles(lineWithoutBeacons.Split('\n'), ref subject);
            
            else if (lineWithoutBeacons.Contains(".gitignore") && parsedPage[i+1].Contains("hljs plaintext language-plaintext"))
            {
                ++i;
                subject.gitignore = lineWithoutBeacons;
            }

            else if (subject.Directories.Count != 0 && subject.FilesWithContent.ContainsKey(subject.Directories[0] + lineWithoutBeacons))
                currentFile = lineWithoutBeacons;
            
            else if (currentFile is not null && 
                    Array.Exists(lineWithoutBeacons.Split('\n'), line => line.Contains("private ") || line.Contains("public ") || line.Contains("protected ") || line.Contains("internal ")))
            {
                if(lineWithoutBeacons.Contains("enum "))
                {
                    subject.FilesWithContent[subject.Directories[0] + currentFile].Add(lineWithoutBeacons);
                    continue;
                }

                foreach(string line in lineWithoutBeacons.Split('\n'))
                {
                    if (!line.Contains("private") && !line.Contains("public") && !line.Contains("protected") &&
                        !line.Contains("internal")) continue;
                    string nextLine = BeaconParse(parsedPage[i + 1], ">", "</code>", false)[0];
                    string precedentLine = BeaconParse(parsedPage[i - 1], ">", "</code>", false)[0];
                    string[] words = line.Split(' ');

                    if (words.Length <= 1) // Pas de mot clé seul
                        continue;

                    if (line.Contains('}') || line[^1] == ';' ||
                        line[^2] == ';' || line[^3] == ';') // Prototype complet
                    {
                        subject.FilesWithContent[subject.Directories[0] + currentFile].Add(line);
                    }
                    else if (words.Length >= 3)
                    {
                        subject.FilesWithContent[subject.Directories[0] + currentFile].Add(line + ";");
                    }
                    else if (nextLine.Split(' ').Length == 1 && nextLine != "private" && nextLine != "public")
                    {
                        subject.FilesWithContent[subject.Directories[0] + currentFile].Add(line + nextLine);
                    }
                    else if (precedentLine.Split(' ').Length == 1 && precedentLine != "private" &&
                             precedentLine != "public")
                    {
                        subject.FilesWithContent[subject.Directories[0] + currentFile].Add(line + precedentLine);
                    }
                }
            }
        }

        return subject;
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

    private static List<string> BeaconParse(string stringToParse, string openingTag, string closingTag, bool includeTags = true)
    {
        int j = 0, openingTagLength = openingTag.Length, closingTagLength = closingTag.Length, resLength = 0;
        StringBuilder line = new StringBuilder();
        bool inBeacon = false;
        List<string> parsedString = new List<string>();
        
        foreach(char c in stringToParse)
        {
            if (!inBeacon)
            {
                if (j < openingTagLength && c == openingTag[j])
                {
                    line.Append(c);
                    ++resLength;
                    ++j;
                }
                else if (j == openingTagLength)
                {
                    inBeacon = true;
                    line.Append(c);
                    ++resLength;
                    j = 0;

                    if (includeTags) continue;
                    line.Remove(resLength - openingTagLength - 1, openingTagLength);
                    resLength -= openingTagLength;
                }
                else if(j != 0)
                {
                    line.Remove(resLength - j, j);
                    resLength -= j;
                    j = 0;
                }
            }
            else
            {
                if (j < closingTagLength && c == closingTag[j])
                {
                    ++j;
                    line.Append(c);
                    ++resLength;
                }
                else if (j == closingTagLength)
                {
                    inBeacon = false;
                    j = 0;

                    if (!includeTags)
                        line.Remove(resLength - closingTagLength, closingTagLength);
                    
                    parsedString.Add(line.ToString());
                    line = line.Clear();
                    resLength = 0;
                    
                    if (c != openingTag[j]) continue;
                    line.Append(c);
                    ++resLength;
                    ++j;
                }
                else
                {
                    j = 0;
                    line.Append(c);
                    ++resLength;
                }
            }
        }

        if (inBeacon)
        {
            int i = resLength - 1;
            while (resLength - closingTagLength < i && line[i] == closingTag[closingTagLength - resLength + i ])
                --i;

            if (i == resLength - closingTagLength && !includeTags)
                line.Remove(resLength - closingTagLength, closingTagLength);
            
            parsedString.Add(line.ToString());
        }
        
        return parsedString;
    }
    
    
    private static string FindFormToken(string htmlCode)
    {
        string tokenLine = BeaconParse(htmlCode, "<input type=\"hidden\"", ">")[0];

        return BeaconParse(tokenLine, "value=\"", "\"",false)[0];
    }

    private static string? FindRepoLink(string forgePageCode)
    {
        foreach (string line in forgePageCode.Split('"'))
        {
            if (line.Contains($"@git.forge.ep{"i"}ta.fr"))
            {
                return line;
            }
        }

        return null;
    }

    private static string NameOfFile(string file)
    {
        StringBuilder name = new StringBuilder();
        bool inName = false;

        for (int i = file.Length - 1; i >= 0; i--)
        {
            if (file[i] == '.')
            {
                inName = true;
                continue;
            }

            if (file[i] == '/')
            {
                break;
            }

            if (inName)
            {
                name.Insert(0, file[i]);
            }
        }

        return name.ToString();
    }

    private static string FindPath(string file)
    {
        int i = file.Length - 1;
        while (i >= 0 && file[i] != '/')
        {
            --i;
        }

        return file[..i];
    }
}