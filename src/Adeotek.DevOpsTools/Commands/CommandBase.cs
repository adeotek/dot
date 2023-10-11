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
    protected readonly string _verboseColor = "aqua";
    protected readonly string _errorColor = "red";
    protected readonly string _warningColor = "olive";
    protected readonly string _standardColor = "turquoise4";
    protected readonly string _successColor = "green";
    protected string? _errOutputColor = "red";
    protected bool MadeChanges;
    protected TSettings? _settings;
    protected bool IsVerbose => _settings?.Verbose ?? false;
    protected bool IsSilent => _settings?.Silent ?? false;
    
    protected abstract int ExecuteCommand(CommandContext context, TSettings settings);
    
    public override int Execute([NotNull] CommandContext context, [NotNull] TSettings settings)
    {
        _settings = settings;
        try
        {
            PrintStart();
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

            PrintDone(e.ExitCode);
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
            
            PrintDone(1);
            return 1;
        }
        finally
        {
            _settings = null;
        }
    }
    
    protected virtual void PrintMessage(string message, string? color = null, bool separator = false, bool skipLineBreak = false)
    {
        if (IsSilent)
        {
            return;
        }
        
        if (separator)
        {
            PrintSeparator();
        }
        
        var composer = new CustomComposer()
            .Style(color ?? _standardColor, message);
        if (!skipLineBreak)
        {
            composer.LineBreak();
        }
        AnsiConsole.Write(composer);
    }
    
    protected virtual void PrintSeparator(bool big = false, bool addEmptyLine = false)
    {
        if (IsSilent)
        {
            return;
        }
        
        var composer = new CustomComposer()
            .Repeat("gray", big ? '=' : '-', _separatorLength).LineBreak();
        if (addEmptyLine)
        {
            composer.LineBreak();
        }
        AnsiConsole.Write(composer);
    }
    
    protected virtual void PrintStart()
    {
        if (IsSilent)
        {
            return;
        }
        
        AnsiConsole.Write(new CustomComposer()
            .Text("Running ").Style("purple", "DOT Container Tool").Space()
            .Style("green", $"v{_version}").LineBreak()
            .Repeat("gray", '=', _separatorLength).LineBreak());
    }
    
    protected virtual void PrintDone(int exitCode = 0)
    {
        var madeChanges = MadeChanges ? "1" : "0";
        if (IsSilent)
        {
            AnsiConsole.Write(exitCode == 0 ? madeChanges : "-1");
            return;
        }
        
        AnsiConsole.Write(new CustomComposer()
            .Repeat("gray", '=', _separatorLength).LineBreak()
            .Style(exitCode == 0 ? "purple" : "gray", $"DONE [{madeChanges}]").LineBreak().LineBreak());
    }
}