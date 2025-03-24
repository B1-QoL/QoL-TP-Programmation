namespace ArchiBuilder;

internal enum Platform
{
    Linux,
    Windows,
    MacOS,
    Undetermined
}

internal class OS
{
    public static Platform DefinePlatform()
    {
#if LINUX
        return Platform.Linux;
#elif WINDOWS
            return Platform.Windows;
#elif MACOS
            return Platform.MacOS;
#else
            return Platform.Undetermined;
#endif
    }
}