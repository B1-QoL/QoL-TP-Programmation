// See https://aka.ms/new-console-template for more information

namespace B1.ArchiBuilder;


/// <summary>
/// The class used to create the TP's tree
/// </summary>
public class ArchiBuilder {
        private readonly HttpClient client = new();

        public void GetSubjectCode(string url, string token) {
            Console.WriteLine("Function in Work in Progess, may not work or cause exceptions");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer <{token}>");

            var request = client.GetAsync(url);

            request.Wait();

            var Request = request.Result;

            var response = Request.Content.ReadAsStringAsync().Result;

            Console.WriteLine(response);
        }

        /// <summary>
        /// Gets the name of a file/directory from a line
        /// </summary>
        /// <param name="line">the line to get the file/Directory from</param>
        /// <returns>The name of the file/directory starting  at the first dot (.) or letter of the line</returns>
        private string getName(string line) {
            var res = "";
            var name = false;
            foreach (var c in line) {
                if (name || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '.') {
                    res += c;
                    name = true;
                }
            }

            return res;
        }

        /// <summary>
        ///  Removes the first line of a string
        /// </summary>
        /// <param name="str">the string to remove the line from</param>
        /// <returns>the string without the first line or an empty string if str is empty or as one line</returns>
        private string removeLine(string str) {
            var split = str.Split('\n');
            var res = "";

            for (var i = 1; i < split.Length; i++) {
                if (i != split.Length - 1)
                    res += split[i] + "\n";
                else
                    res += split[i];
            }

            return res;
        }

        /// <summary>
        /// Gets the first line of a string
        /// </summary>
        /// <param name="str">The string to get the line from</param>
        /// <returns>The first line of the string</returns>
        private string getLine(string str) {
            var split = str.Split('\n');

            var res = "";

            if (split.Length >= 1) res = split[0];


            return res;
        }

        /// <summary>
        /// Gets the depth in the tree of a line using the │ (\u2502) caracter
        /// </summary>
        /// <param name="line">The line to get the depth from</param>
        /// <returns>an integer representing the depth of the file/folder</returns>
        private int getDepth(string line) {
            var res = 0;

            foreach (var c in line)
                if (c == '\u2502')
                    res++;

            return res;
        }

        /// <summary>
        /// Creates the whole file system from the tree in the current working directory 
        /// </summary>
        /// <param name="tree">the tree to build</param>
        /// <remarks>
        /// Does not create any .sln file. Also treats any path with a dot (.) as a file
        /// </remarks>
        public void CreateArchi(string tree) {
            if (Directory.GetCurrentDirectory().Length >= 16 &&
                Directory.GetCurrentDirectory()[^16..] == "bin\\Debug\\net7.0") {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(
                    "Warning: working directory may not be near the .sln file, may need to remove the trailing bin/Debug/net7.0 from the working directory");
                Console.ResetColor();
            }

            var nomTP = "";

            var stack = new Stack<(string, int)>();

            while (tree != "") {
                tree = removeLine(tree);
                var line = getLine(tree);
                var depth = getDepth(line);
                var name = getName(line);

                if (nomTP == "") {
                    nomTP = name;
                }

                if (!name.Contains('.') && name != "README") {
                    name += "/";

                    var path = "";
                    var dirplaced = false;
                    
                    while (stack.Count != 0 && !dirplaced) {
                        var (prevP, prevD) = stack.Peek();
                        if (prevD < depth) {
                            path = prevP;
                            dirplaced = true;
                        }
                        else {
                            stack.Pop();
                        }
                    }
                    
                    if (stack.Count != 0) {
                        var (prevP, _) = stack.Peek();
                        var split = prevP.Split('/');
                        if (prevP.Length >= 2) {
                            if (split[^2] == "Tests") {
                                File.Create(path + name[..^1] + ".info").Dispose();
                                
                                using (var sw = new StreamWriter(path + name[..^1] + ".useless")) {
                                    sw.Write("Put your test files in the same directory as this file\n\nThis file can be removed at any time.");
                                }
                                
                                continue;
                            }
                        }
                    }
                    
                    stack.Push((path + name, depth));
                    Directory.CreateDirectory(path + name);
                    
                }
                else if (name.Contains('.')) {
                    var ext = name.Split('.')[^1];

                    var content = "";

                    var path = "";
                    var fileplaced = false;

                    while (stack.Count != 0 && !fileplaced) {
                        var (prevN, prevD) = stack.Peek();
                        if (prevD < depth) {
                            path = prevN;
                            fileplaced = true;
                        }
                        else {
                            stack.Pop();
                        }
                    }

                    switch (ext) {
                        case "cs":
                            content =
                                $"namespace {path.Replace("/", ".")[..^1]};\n\npublic class {name.Replace("." + ext, "")} {{\n\n\n}}\n\n";
                            break;
                        case "csproj":
                            content =
                                "<Project Sdk=\"Microsoft.NET.Sdk\">\n\n    <PropertyGroup>\n        <OutputType>Exe</OutputType>\n        <TargetFramework>net7.0</TargetFramework>\n        <ImplicitUsings>enable</ImplicitUsings>\n        <Nullable>enable</Nullable>\n    </PropertyGroup>\n\n</Project>";
                            break;
                        case "gitignore":
                            content = "bin/\nobj/\n\n.idea/\n*~\n*.DotSettings.user";
                            break;
                        case "sln":
                            // Process.Start("cmd.exe", $"dotnet new sln --name {name.Replace("." + ext, "")}");
                            continue;
                    }

                    using (var sw = new StreamWriter(path + name)) {
                        sw.Write(content);
                    }

                }
                else {
                    if (name == "README") {
                        var content = $"File system of {nomTP} created by B1's EpiCommunist's archi builder";

                        var path = "";
                        var fileplaced = false;

                        while (stack.Count != 0 && !fileplaced) {
                            var (prevN, prevD) = stack.Peek();
                            if (prevD < depth) {
                                path = prevN;
                                fileplaced = true;
                            }
                            else {
                                stack.Pop();
                            }
                        }

                        using (var sw = new StreamWriter(path + name)) {
                            sw.Write(content);
                        }
                    }
                }
            }
        }
    }

