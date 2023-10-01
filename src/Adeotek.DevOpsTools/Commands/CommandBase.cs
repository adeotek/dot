using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Common;
using Adeotek.DevOpsTools.Extensions;
using Adeotek.Extensions.Processes;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class CommandBase<TSettings> : Command<TSettings> where TSettings : GlobalSettings
{
    protected readonly string _version = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "?";
    protected readonly int _separatorLength = 80;
    protected readonly string _errorColor = "red";
    protected readonly string _warningColor = "olive";
    protected readonly string _standardColor = "turquoise4";
    protected readonly string _successColor = "green";
    protected string? _errOutputColor = "red";
    protected TSettings? _settings;
    protected bool IsVerbose => _settings?.Verbose ?? false;
    
    protected abstract int ExecuteCommand(CommandContext context, TSettings settings);
    
    public override int Execute([NotNull] CommandContext context, [NotNull] TSettings settings)
    {
        try
        {
            PrintStart();
            _settings = settings;
            var exitCode = ExecuteCommand(context, settings);
            PrintDone();
            return exitCode;
        }
        catch (ShellCommandException e)
        {
            if (IsVerbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
            else
            {
                e.WriteToAnsiConsole();
            }

            return e.ExitCode;
        }
        catch (Exception e)
        {
            if (IsVerbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
            else
            {
                e.WriteToAnsiConsole();
            }

            return 1;
        }
        finally
        {
            _settings = null;
        }
    }
    
    protected virtual void PrintMessage(string message, string? color = null, bool separator = false)
    {
        if (separator)
        {
            PrintSeparator();
        }
        AnsiConsole.Write(new CustomComposer()
            .Style(color ?? _standardColor, message).LineBreak());
    }
    
    protected virtual void PrintSeparator(bool big = false)
    {
        AnsiConsole.Write(new CustomComposer()
            .Repeat("gray", big ? '=' : '-', _separatorLength).LineBreak());
    }
    
    protected virtual void PrintStart()
    {
        AnsiConsole.Write(new CustomComposer()
            .Text("Running ").Style("purple", "DOT Container Tool").Space()
            .Style("green", $"v{_version}").LineBreak()
            .Repeat("gray", '=', _separatorLength).LineBreak());
    }
    
    protected virtual void PrintDone()
    {
        AnsiConsole.Write(new CustomComposer()
            .Repeat("gray", '=', _separatorLength).LineBreak()
            .Style("purple", "DONE.").LineBreak().LineBreak());
    }
}