namespace ArchiBuilder;

internal struct Subject
{
    public string RepositoryLink;
    public List<string> CreationCommands;
    public List<string> Directories;
    public Dictionary<string, List<string>> FilesWithContent;
    public string ProgramPath;
    public string gitignore;
    public List<string> Examples;
    public string FolderTP;

    public Subject()
    {
        CreationCommands = new List<string>();
        Directories = new List<string>();
        FilesWithContent = new Dictionary<string, List<string>>();
        ProgramPath = "";
        gitignore = "";
        Examples = new List<string>();
        FolderTP = "";
    }
}