using System.Text;

namespace B1;

public static class Parsing
{
    /// <summary>
    /// Méthode séparant une chaîne de caractères en fonction d'une balise de début et de fin.
    /// </summary>
    /// <param name="stringToParse">La chaîne de caractères à séparer.</param>
    /// <param name="openingTag">La balise ouvrante.</param>
    /// <param name="closingTag">La balise fermante.</param>
    /// <param name="includeTags">Si <c>includeTags</c> vaut <c>false</c>, les balises seront retirées du résultat.</param>
    /// <returns>Une <c>List&lt;string&gt;</c> avec tous les éléments trouvés entre les balises.</returns>
    /// <exception cref="ArgumentException">Les balises ne peuvent être vides.</exception>
    public static List<string> BeaconParse(string stringToParse, string openingTag, string closingTag, bool includeTags = true)
    {
        if (openingTag == "" || closingTag == "")
        {
            throw new ArgumentException("Les balises ne peuvent être vides.");
        }
        
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
            while (resLength - closingTagLength < i && line[i] == closingTag[closingTagLength - resLength + i])
                --i;

            if (line[^1] == closingTag[^1] && i == resLength - closingTagLength && !includeTags)
                line.Remove(resLength - closingTagLength, closingTagLength);
            
            parsedString.Add(line.ToString());
        }
        
        return parsedString;
    }
}