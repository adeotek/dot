using System.Diagnostics.CodeAnalysis;

namespace Adeotek.Extensions.Processes;

[ExcludeFromCodeCoverage]
public class ShellCommandException : Exception
{
    public int ExitCode { get; }

    public ShellCommandException(int exitCode, string message) 
        : base(message)
    {
        ExitCode = exitCode;
    }
    
    public ShellCommandException(int exitCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ExitCode = exitCode;
    }
}