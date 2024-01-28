using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Containers.Exceptions;

[ExcludeFromCodeCoverage]
public class ContainersCliException : ShellCommandException
{
    public string Command { get; }

    public ContainersCliException(string command, int exitCode, string message) 
        : base(exitCode, message)
    {
        Command = command;
    }
    
    public ContainersCliException(string command, int exitCode, string message, Exception innerException) 
        : base(exitCode, message, innerException)
    {
        Command = command;
    }
    
    public static void ThrowIf(bool condition, string command, string message, int exitCode = 1)
    {
        if (condition)
        {
            Throw(command, exitCode, message);
        }
    }
    
    public static void ThrowIfNull([NotNull] object? argument, string command, string message, int exitCode = 1)
    {
        if (argument is null)
        {
            Throw(command, exitCode, message);
        }
    }
    
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            Throw("null", 1, $"Null argument {paramName}");
        }
    }
    
    [DoesNotReturn]
    public static void Throw(string command, int exitCode, string message) =>
        throw new ContainersCliException(command, exitCode, message);
}