using System.Diagnostics;

namespace ArchiBuilder;

public static partial class ArchiBuilder
{
    private static List<string> CheckExistingFiles(Subject subject)
    {
        List<string> existingFiles = new List<string>();
        foreach (KeyValuePair<string,List<string>> file in subject.FilesWithContent)
        {
            if (File.Exists(file.Key))
                existingFiles.Add(file.Key);
        }

        return existingFiles;
    }

    private static bool CheckRights()
    {
        try
        {
            string testFile = "checkingRights.txt";
            File.Create(testFile);
            File.Delete(testFile);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
    
    private static void WriteSubject(Subject subject, bool cloneRepository, bool createProject)
    {
        if (cloneRepository)
        {
            Console.WriteLine("Clonage du dépôt...");
            int i = 0;
            while (i < 10 && !Directory.Exists(subject.FolderTP)) // La Forge peut avoir quelques probleme de connexion.
            {
                Process? process = CreateProcess($"GIT_SSH_COMMAND='ssh -o StrictHostKeyChecking=no' git clone {subject.RepositoryLink}");
                if (process is null)
                    throw new ArchiBuilderExceptions.CloneException();
                process.WaitForExit();
                ++i;
            }

            if (i == 10)
                throw new ArchiBuilderExceptions.CloneException();

            Directory.SetCurrentDirectory(subject.FolderTP);
            Console.WriteLine("Dépôt cloné !");
        }
        
        AskForExistingFile(subject);
        
        if (createProject)
        {
            foreach (string command in subject.CreationCommands)
            {
                Process? process = CreateProcess(command);
                if (process is null)
                    throw new ArchiBuilderExceptions.CloneException();
                process.WaitForExit();
            }
        }

        foreach (string directory in subject.Directories)
        {
            Directory.CreateDirectory(directory);
        }

        foreach ((string file, List<string> content) in subject.FilesWithContent)
        {
            using (StreamWriter streamWriter = new StreamWriter(file))
            {
                if (file == "README")
                {
                    streamWriter.Write(content[0]);
                    continue;
                }
                
                streamWriter.Write("/*");
                streamWriter.WriteLine($"namespace {FindPath(file).Replace('/','.')};\n"); // Ne conserver que le nom du dossier
                bool inContainer = false;
                bool lastIsEnum = false;
                string pending = "";
                
                foreach (string prototype0 in content)
                {
                    string prototype = prototype0.Trim();
                    
                    if(inContainer)
                    {
                        if (IsContainer(prototype))
                        {
                            
                            streamWriter.WriteLine("}\n");
                            streamWriter.WriteLine(RemoveAfter(prototype,'{'));
                            streamWriter.WriteLine("{");
                            lastIsEnum = false;
                        }
                        else if(prototype.Contains("enum"))
                        {

                            inContainer = false;
                            streamWriter.WriteLine("}\n");
                            streamWriter.WriteLine(prototype);
                            lastIsEnum = true;
                        }
                        else
                        {
                            streamWriter.WriteLine("\t//" + prototype);
                            lastIsEnum = false;
                        }
                    }
                    else 
                    {
                        if (IsContainer(prototype))
                        {
                            streamWriter.WriteLine(RemoveAfter(prototype,'{'));
                            streamWriter.WriteLine("{");
                            inContainer = true;
                            lastIsEnum = false;
                            streamWriter.Write(pending);
                            pending = "";
                        }
                        else if (prototype.Contains("enum"))
                        {
                            streamWriter.WriteLine(prototype);
                            lastIsEnum = true;
                        }
                        else
                        {
                            pending += "\t//" + prototype + "\n";
                        }
                    }
                }

                if (pending != "")
                {
                    streamWriter.Write("public ");
                    if(content.TrueForAll(s => s.Contains("static")))
                        streamWriter.Write("static ");
                    else if(content.Exists( s => s.Contains("abstract")))
                        streamWriter.Write("abstract ");
                    if(NameOfFile(file)[0] == 'I' && NameOfFile(file)[1] is >= 'A' and <='Z')
                        streamWriter.Write("interface ");
                    else
                        streamWriter.Write("class ");
                    streamWriter.WriteLine(NameOfFile(file));
                    streamWriter.WriteLine("{");
                    streamWriter.Write(pending);
                }
                
                if (!lastIsEnum)
                    streamWriter.Write("}");
                
                streamWriter.Write("*/");
            }
        }
        
        using(StreamWriter sw = new StreamWriter(".gitignore"))
        {
            sw.Write("bin/\nobj/\n\n.idea/\n*~\n*.DotSettings.user");
        }

        if (subject.ProgramPath == "")
            subject.ProgramPath = subject.Directories[0] + "Program.cs";
        
        using (StreamWriter streamWriter = new StreamWriter(subject.ProgramPath))
        {
            streamWriter.WriteLine("using static B1.Affichage;");
            streamWriter.WriteLine($"//using {subject.Directories[0][..^1]};\n");
            streamWriter.WriteLine("Print(\"Ce TP a été généré avec amour par trois de vos camarades.\");");
            streamWriter.WriteLine("Print(\"En espérant qu'il vous soit enrichissant.\");");
            streamWriter.WriteLine("Print();");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, new object[] {}, null, null, new object[] {}, null, null, new object[] {},new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, null, new object[] {}, new object[] {}, null, new object[] {}, new object[] {}, null,new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { null, new object[] {}, new object[] {}, new object[] {}, null, new object[] {}, new object[] {}, new object[] {}, null},true);");
            streamWriter.WriteLine("Print( new object?[] { null, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, null},true);");
            streamWriter.WriteLine("Print( new object?[] { null, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, null},true);");
            streamWriter.WriteLine("Print( new object?[] { null, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, null},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, null, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, null,new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, null, new object[] {}, new object[] {}, new object[] {}, new object[] {}, new object[] {}, null,new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, new object[] {}, null, new object[] {}, new object[] {}, new object[] {}, null, new object[] {},new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, new object[] {}, null, new object[] {}, new object[] {}, new object[] {}, null, new object[] {},new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, new object[] {}, new object[] {}, null, new object[] {}, null, new object[] {}, new object[] {},new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, new object[] {}, new object[] {}, null, new object[] {}, null, new object[] {}, new object[] {},new object[] {}},true);");
            streamWriter.WriteLine("Print( new object?[] { new object[] {}, new object[] {}, new object[] {}, new object[] {}, null, new object[] {}, new object[] {}, new object[] {},new object[] {}},true);");
        }
    }

    private static bool IsContainer(string prototype) =>
        prototype.Contains("class") ||
        /*prototype.Contains("enum") ||*/ //Les énumérations sont généralement soit complètement déclarés soit décrits dans le sujet (donc intraitables)
        prototype.Contains("interface") ||
        prototype.Contains("struct") ||
        prototype.Contains("record");
    
    private static Process? CreateProcess(string command)
    {
        ProcessStartInfo processStartInfo = new ()
        {
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        switch (OS.DefinePlatform())
        {
            case Platform.Linux: case Platform.MacOS:
                processStartInfo.FileName = "/bin/sh";
                processStartInfo.Arguments = $"-c \"{command}\"";
                break;
            case Platform.Windows:
                processStartInfo.FileName = @"%windir%\system32\cmd.exe";
                processStartInfo.Arguments = $"/c \"{command}\"";
                break;
            default:
                throw new PlatformNotSupportedException();
        }
        
        return Process.Start(processStartInfo);
    }

    private static void AskForExistingFile(Subject subject) 
    {
        foreach (string file in CheckExistingFiles(subject))
        {
            Console.WriteLine($"Ce fichier existe déjà : {file}. Souhaitez-vous le modifier ? (o/n)");

            if (Console.ReadKey().KeyChar == 'o')
            {
                try
                {
                    File.OpenWrite(file).Close();
                }
                catch (Exception)
                {
                    Console.WriteLine("Le fichier n'a pas pu être modifié.");
                    subject.FilesWithContent.Remove(file);
                }
            }
            else
                subject.FilesWithContent.Remove(file);
            
            Console.WriteLine();
        }
    }

}