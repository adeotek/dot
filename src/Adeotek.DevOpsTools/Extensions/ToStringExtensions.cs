using Adeotek.DevOpsTools.Common;
using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Processes;

using Spectre.Console;

namespace Adeotek.DevOpsTools.Extensions;

internal static class AnsiConsolePrintExtensions
{
    private const int NameLength = 18;
    private const int SubValueIndent = 2;
    private const char SubValuePrefix = ' ';
    private const string MappingSeparator = " -> ";
    private const string Null = "[null]";
    private const string LabelColor = "gray";
    private const string ValueColor = "aqua";
    private const string SpecialValueColor = "turquoise4";
    private const string SpecialColor = "teal";
    
    internal static void WriteToAnsiConsole(this ContainerConfigV1 configV1)
    {
        var composer = new CustomComposer()
            .Style(SpecialColor, "[ContainerConfig]").LineBreak()
            .Text("ContainerName:", NameLength).Style(SpecialValueColor, configV1.CurrentName).LineBreak()
            .Text("FullImageName:", NameLength).Style(SpecialValueColor, configV1.FullImageName).LineBreak()
            .Style(LabelColor, "Image:", NameLength).Style(ValueColor, configV1.Image).LineBreak()
            .Style(LabelColor, "Tag:", NameLength).Style(ValueColor, configV1.Tag ?? Null).LineBreak()
            .Style(LabelColor, "NamePrefix:", NameLength).Style(ValueColor, configV1.NamePrefix ?? Null).LineBreak()
            .Style(LabelColor, "Name:", NameLength).Style(ValueColor, configV1.Name).LineBreak()
            .Style(LabelColor, "CurrentSuffix:", NameLength).Style(ValueColor, configV1.CurrentSuffix ?? Null).LineBreak()
            .Style(LabelColor, "PreviousSuffix:", NameLength).Style(ValueColor, configV1.PreviousSuffix ?? Null).LineBreak()
            .Style(LabelColor, "Ports:").LineBreak().AddConfigPorts(configV1.Ports)
            .Style(LabelColor, "Volumes:").LineBreak().AddConfigVolumes(configV1.Volumes)
            .Style(LabelColor, "EnvVars:").LineBreak().AddConfigEnvVars(configV1.EnvVars)
            .Style(LabelColor, "Network:").LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Name:", NameLength - SubValueIndent).Style(ValueColor, configV1.Network?.Name ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Subnet:", NameLength - SubValueIndent).Style(ValueColor, configV1.Network?.Subnet ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "IpRange:", NameLength - SubValueIndent).Style(ValueColor, configV1.Network?.IpRange ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "IpAddress:", NameLength - SubValueIndent).Style(ValueColor, configV1.Network?.IpAddress ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Hostname:", NameLength - SubValueIndent).Style(ValueColor, configV1.Network?.Hostname ?? Null).LineBreak()
            .Repeat(SubValuePrefix, SubValueIndent)
            .Style(LabelColor, "Alias:", NameLength - SubValueIndent).Style(ValueColor, configV1.Network?.Alias ?? Null).LineBreak()
            .Style(LabelColor, "ExtraHosts:").LineBreak().AddConfigExtraHosts(configV1.ExtraHosts)
            .Style(LabelColor, "Restart:", NameLength).Style(ValueColor, configV1.Restart ?? Null).LineBreak();

        AnsiConsole.Write(composer);
    }

    internal static CustomComposer AddConfigPorts(this CustomComposer composer, PortMappingV1[] ports)
    {
        if (ports.Length == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach (var port in ports)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(SpecialValueColor, port.Host.ToString())
                .Text(MappingSeparator).Style(ValueColor, port.Container.ToString())
                .LineBreak();
        }

        return composer;
    }
    
    internal static CustomComposer AddConfigVolumes(this CustomComposer composer, VolumeConfigV1[] volumes)
    {
        if (volumes.Length == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach (var port in volumes)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(SpecialValueColor, port.Source)
                .Text(MappingSeparator).Style(ValueColor, port.Destination).Text(" (")
                .Style(port.IsBind ? "green" : "red",$"IsBind={(port.IsBind ? "Yes" : "No")}").Text(" | ")
                .Style(port.IsReadonly ? "green" : "red",$"IsReadonly={(port.IsReadonly ? "Yes" : "No")}").Text(" | ")
                .Style(port.AutoCreate ? "green" : "red",$"AutoCreate={(port.AutoCreate ? "Yes" : "No")}")
                .Text(")").LineBreak();
        }

        return composer;
    }
    
    internal static CustomComposer AddConfigEnvVars(this CustomComposer composer, Dictionary<string, string> envVars)
    {
        if (envVars.Count == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach ((string key, string value) in envVars)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(SpecialValueColor, key)
                .Text("=").Style(ValueColor, value).LineBreak();
        }

        return composer;
    }
    
    internal static CustomComposer AddConfigExtraHosts(this CustomComposer composer, Dictionary<string, string> extraHosts)
    {
        if (extraHosts.Count == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach ((string key, string value) in extraHosts)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(SpecialValueColor, key)
                .Text(":").Style(ValueColor, value).LineBreak();
        }

        return composer;
    }

    internal static void WriteToAnsiConsole(this ShellCommandException exception)
    {
        AnsiConsole.MarkupLineInterpolated($"[purple]ERROR({exception.ExitCode}):[/] [red]{exception.Message}[/]");
    }
    
    internal static void WriteToAnsiConsole(this Exception exception)
    {
        AnsiConsole.MarkupLineInterpolated($"[purple]ERROR(1):[/] [red]{exception.Message}[/]");
    }
}