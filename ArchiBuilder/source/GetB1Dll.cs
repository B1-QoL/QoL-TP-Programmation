using System.Diagnostics;

namespace ArchiBuilder;

public partial class ArchiBuilder 
{
    private static void GetB1Dll(Subject subject) 
    {
        CloneB1Dll();
        WriteCsproj(subject);
    }

    private static void CloneB1Dll() 
    {
        if (Directory.Exists("../QoL-TP-Programmation")) return;


        Console.WriteLine("Clonage de B1.dll...");
        
        Process? process =
            CreateProcess(
                "cd ..; " +
                "git clone -n --depth=1 --filter=tree:0 https://github.com/B1-QoL/QoL-TP-Programmation.git; " +
                "cd QoL-TP-Programmation; " +
                "git sparse-checkout set --no-cone Bibliothèque/bin; " +
                "git checkout"
            );

        process.WaitForExit();
        
        Console.WriteLine("Clonage du dépôt effectué.");
    }

    private static void WriteCsproj(Subject subject) 
    {
        string content = "";
        
        using (StreamReader sr = new StreamReader($"{subject.Directories[0]}{subject.Directories[0][..^1]}.csproj")) 
        {
            content = sr.ReadToEnd();
        }
        
        if (content.Contains("<Reference Include=\"B1\">")) return;


        List<string> parsed = BeaconParse(content, "<Project Sdk=\"Microsoft.NET.Sdk\">", "</Project>", false);
        
        parsed.Insert(0, "<Project Sdk=\"Microsoft.NET.Sdk\">\n");
        parsed.Add(
            "\n\t<ItemGroup>\n\t\t<Reference Include=\"B1\">\n\t\t<HintPath>..\\..\\QoL-TP-Programmation\\Bibliothèque\\bin\\B1.dll</HintPath>\n\t\t</Reference>\n\t</ItemGroup>");
        parsed.Add("\n</Project>");
        
        using (StreamWriter sw = new StreamWriter($"{subject.Directories[0]}{subject.Directories[0][..^1]}.csproj")) 
        {
            foreach (var line in parsed) 
                sw.Write(line);
        }
    }
}