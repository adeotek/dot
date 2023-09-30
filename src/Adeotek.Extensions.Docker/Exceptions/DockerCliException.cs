using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Docker.Exceptions;

public class DockerCliException : ShellCommandException
{
    public string Command { get; }

    public DockerCliException(string command, int exitCode, string message) 
        : base(exitCode, message)
    {
        Command = command;
    }
    
    public DockerCliException(string command, int exitCode, string message, Exception innerException) 
        : base(exitCode, message, innerException)
    {
        Command = command;
    }
}