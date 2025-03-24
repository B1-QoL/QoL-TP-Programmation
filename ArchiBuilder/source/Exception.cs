namespace ArchiBuilder;

public class ArchiBuilderExceptions
{
    public class EmptyLoginException : Exception
    {
        public EmptyLoginException() : base() {}
    }

    public class EmptyPasswordException : Exception
    {
        public EmptyPasswordException() : base() {}
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base() {}
    }
    
    public class CloneException : Exception
    {
        public CloneException() : base() {}
    }
}