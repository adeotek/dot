namespace Adeotek.Extensions.ConfigFiles;

public class ConfigFileException : Exception
{
    public string? File { get; }

    public ConfigFileException(string message, string? file = null) 
        : base(message)
    {
        File = file;
    }
    
    public ConfigFileException(string message, string? file, Exception innerException) 
        : base(message, innerException)
    {
        File = file;
    }
}