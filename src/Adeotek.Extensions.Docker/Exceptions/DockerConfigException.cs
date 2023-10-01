using System.Diagnostics.CodeAnalysis;

namespace Adeotek.Extensions.Docker.Exceptions;

[ExcludeFromCodeCoverage]
public class DockerConfigException : Exception
{
    public string? File { get; }

    public DockerConfigException(string message, string? file = null) 
        : base(message)
    {
        File = file;
    }
    
    public DockerConfigException(string message, string? file, Exception innerException) 
        : base(message, innerException)
    {
        File = file;
    }
}