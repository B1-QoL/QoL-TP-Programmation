namespace ArchiBuilder;

public static partial class ArchiBuilder
{
    public static int Main(string[] args) 
    {
        #region GestionOS

        List<Warnings> warnings = new List<Warnings>();
        Platform platform = OS.DefinePlatform();
        bool cloneRepository = true;
        bool createProject = true;

        switch (platform) 
        {
            case Platform.Undetermined:
                Console.WriteLine("Votre système d'exploitation n'est pas pris en charge.");
                return 1;
            case Platform.Windows:
                warnings.Add(Warnings.Windows);
                break;
            case Platform.MacOS:
                warnings.Add(Warnings.MacOS);
                break;
        }

        #endregion

        #region GestionParamètres

        switch (args.Length)
        {
            case 0 or > 3:
                Help();
                return 2; // Erreurs sur les paramètres d'appel.
            case >= 2:
            {
                switch (args[0])
                {
                    case "-l":
                        cloneRepository = false;
                        break;
                    case "-d":
                        createProject = false;
                        break;
                    default:
                        Help();
                        return 2; // Erreurs sur les paramètres d'appel.
                }

                if (args.Length == 3)
                {
                    switch (args[0],args[1])
                    {
                        case ("-l","-d"): case ("-d","-l"):
                            cloneRepository = false;
                            createProject = false;
                            break;
                        default:
                            Help();
                            return 2; // Erreurs sur les paramètres d'appel.
                    }
                }

                break;
            }
        }

        string forgeLink = args[^1];

        if(!forgeLink.StartsWith($"https://intra.forge.ep{"i"}ta.fr/ep{"i"}ta-prepa-computer-science/"))
        {
            Console.WriteLine("Le lien entré n'est pas celui d'un TP.");
            return 3; // Lien invalide.
        }
        
        #endregion

        #region Identifiants

        CriUser? user = null;
        try
        {
            user = CriUser.AskCredentials();
        }
        catch (Exception exception)
        {
            switch (exception)
            {
                case ArchiBuilderExceptions.EmptyLoginException :
                    Console.WriteLine("Le champ 'Login' ne doit pas être vide.");
                    return 4;
                case ArchiBuilderExceptions.EmptyPasswordException :
                    Console.WriteLine("Le champ 'Password' ne doit pas être vide.");
                    return 5;
            }
        }
        #endregion
        
        #region PageForge

        (string? forgePageCode, string? subjectPageCode) = (null, null);
        try
        {
            (forgePageCode, subjectPageCode) = GetSubject(forgeLink, user!);
        }
        catch(Exception exception)
        {
            switch(exception)
            {
                case NotSupportedException:
                    Console.WriteLine("La version HTTP n'est pas prise en charge.");
                    return 6;
                case HttpRequestException:
                    Console.WriteLine("Il y a un problème de connection au site. Êtes-vous sur d'être connecté à Internet ?");
                    return 7;
                case TaskCanceledException:
                    Console.WriteLine("Le délai de connection est trop important. La requête a été annulée.");
                    return 8;
                case OutOfMemoryException:
                    Console.WriteLine("Il n'y a pas assez de place en mémoire pour exécuter le programme.");
                    return 9;
                case ArchiBuilderExceptions.InvalidCredentialsException:
                    Console.WriteLine("Identifiants invalides.");
                    return 10;
            }
        }

        #endregion

        #region Clonage

        // Trouver le lien du dépôt git
        string? repoLink = FindRepoLink(forgePageCode!);
        if (repoLink is null)
        {
            Console.WriteLine("Aucun lien de dépôt git n'a été trouvé à l'adresse donnée. L'adresse est-elle valide ?");
            return 11;
        }

        #endregion

        #region AnalyseDeLaPage

        List<string> parsedPage = BeaconParse(subjectPageCode!, "<code", "</code>");

        Subject subject = ParseSubject(parsedPage);
        subject.RepositoryLink = repoLink;
        subject.FolderTP = BeaconParse(repoLink, "epita-prepa-computer-science-prog", ".git")[0][..^4];

        if (subject.FilesWithContent.Count == 0)
        {
            Console.WriteLine("Aucun fichier ou prototype détecté. L'adresse est-elle correcte ?");
            return 12; // Pas de fichier ou prototype trouvé.
        }

        #endregion

        #region CréationDuTP

        List<string> defaultCreationCommands = new List<string>
        {
            $"dotnet new sln --name {subject.Directories[0]}",
            $"dotnet new console -n {subject.Directories[0]} -f net7.0 -lang 'C#'",
            $"dotnet sln add {subject.Directories[0]}/{subject.Directories[0]}.csproj"
        };

        if (subject.CreationCommands.Count == 0)
        {
            warnings.Add(Warnings.MissingCreationCommands);
            subject.CreationCommands = defaultCreationCommands;
        }

        #endregion
       
        #region ÉcritureDuTP

        if (CheckRights()) 
        {
            try 
            {
                Console.WriteLine("Création du TP...");
                WriteSubject(subject,cloneRepository,createProject);
                GetB1Dll(subject);
                
            }
            catch (Exception exception) 
            {
                if (exception is ArchiBuilderExceptions.CloneException)
                {
                    Console.WriteLine("Un problème est survenu lors du clonage du dépôt.");
                    return 13;
                }

                Console.WriteLine("Un problème est survenu lors de la création et l'écriture des fichiers.");
                return 14;
            }
        }
        else
        {
            Console.WriteLine("L'utilisateur n'a pas les droits pour écrire dans ce dossier");
            return 15;
        }

        #endregion

        #region Warnings

        int finalReturn = 0;

        foreach (Warnings warning in warnings)
        {
            switch (warning)
            {
                case Warnings.MissingCreationCommands:
                    Console.WriteLine("ATTENTION : Aucune commande de création de projet C# trouvé. Celles par défaut ont été utilisées.");
                    foreach (string command in defaultCreationCommands)
                        Console.WriteLine(command);
                    finalReturn -= 1;
                    break;
                case Warnings.Windows:
                    Console.WriteLine("ATTENTION : Il semblerais que vous utilisiez Windows. C'est un signe de grande infériorité intellectuelle et vous prenez le risque de finir dans votre vie soûl dans un caniveau. Considérez à passer sous Linux.");
                    finalReturn -= 2;
                    break;
                case Warnings.MacOS:
                    Console.WriteLine("Si j'avais su que mon programme allait servir sur un Mac... Je pense que j'aurais fait une directive préprocesseur pour lui dire d'aller s'acheter un vrai PC.");
                    finalReturn -= 4;
                    break;
            }
        }

        #endregion

        Console.WriteLine("TP généré avec succès !");
        return finalReturn;
    }

    private static void Help()
    {
        Console.WriteLine("Syntaxe invalide.\n");
        
        Console.WriteLine("ArchiBuilder est un programme créant un squelette de TP à partir de vos identifiants Forge.\n");

        Console.WriteLine("Utilisation :");
        Console.WriteLine("\tArchiBuilder *lien du TP*");
        Console.WriteLine("\t\tClone le dépôt git dans le dossier courant et crée le TP dedans.\n");

        Console.WriteLine("\tArchiBuilder -l *lien du TP*");
        Console.WriteLine("\t\tCrée le TP dans le dossier courant sans cloner le dépôt git.\n");
        Console.WriteLine("\tArchiBuilder -d *lien du TP*");
        Console.WriteLine("\t\tNe pas créer le projet.\n");

        Console.WriteLine("Informations :");
        Console.WriteLine("\t- Si un fichier existe déjà dans le dossier de création, ArchiBuilder vous proposera de le conserver dans son état ou de le réécrire.");
        Console.WriteLine("\t- ArchiBuilder met à disposition une bibliothèque de fonctions utiles. Vous pouvez les utiliser sans restriction dans Program.cs mais pas dans les autres fichiers. Pour plus d'informations, consultez le github du projet.");
    }
}