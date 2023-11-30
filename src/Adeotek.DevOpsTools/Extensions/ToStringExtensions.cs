using Adeotek.DevOpsTools.Common;
using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Processes;

using Spectre.Console;

namespace Adeotek.DevOpsTools.Extensions;

internal static class AnsiConsolePrintExtensions
{
    private const int NameLength = 20;
    private const int SubValueIndent = 2;
    private const char SubValuePrefix = ' ';
    private const string MappingSeparator = " -> ";
    private const string Null = "[null]";
    private const string LabelColor = "gray";
    private const string ValueColor = "aqua";
    private const string SpecialValueColor = "turquoise4";
    private const string SpecialColor = "teal";
    
    internal static void WriteToAnsiConsole(this ContainersConfig config)
    {
        var composer = new CustomComposer()
            .Style(SpecialColor, "[ContainersConfig]").LineBreak()
            .Style(SpecialValueColor, "- [Services]").LineBreak();
        foreach ((string name, ServiceConfig service) in config.Services)
        {
            composer.Style(SpecialColor, $"-- <{name}>").LineBreak()
                .AddServiceConfig(service);
        }
        composer.Style(SpecialValueColor, "- [Networks]").LineBreak();
        foreach ((string name, NetworkConfig network) in config.Networks)
        {
            composer.Style(SpecialColor, $"-- <{name}>").LineBreak()
                .AddNetworkConfig(network);
        }
        composer.LineBreak();

        AnsiConsole.Write(composer);
    }

    internal static CustomComposer AddServiceConfig(this CustomComposer composer, ServiceConfig service)
    {
        composer.Text("CurrentName:", NameLength).Style(SpecialValueColor, service.CurrentName).LineBreak()
            .Text("PreviousName:", NameLength).Style(SpecialValueColor, service.PreviousName ?? "N/A").LineBreak()
            .Style(LabelColor, "Image:", NameLength).Style(ValueColor, service.Image).LineBreak()
            .Style(LabelColor, "PullPolicy:", NameLength).Style(ValueColor, service.PullPolicy ?? Null).LineBreak()
            .Style(LabelColor, "ContainerName:", NameLength).Style(ValueColor, service.ContainerName ?? Null).LineBreak()
            .Style(LabelColor, "NamePrefix:", NameLength).Style(ValueColor, service.NamePrefix ?? Null).LineBreak()
            .Style(LabelColor, "BaseName:", NameLength).Style(ValueColor, service.BaseName ?? Null).LineBreak()
            .Style(LabelColor, "CurrentSuffix:", NameLength).Style(ValueColor, service.CurrentSuffix ?? Null).LineBreak()
            .Style(LabelColor, "PreviousSuffix:", NameLength).Style(ValueColor, service.PreviousSuffix ?? Null).LineBreak()
            .Style(LabelColor, "Ports:").LineBreak().AddConfigPorts(service.Ports)
            .Style(LabelColor, "Volumes:").LineBreak().AddConfigVolumes(service.Volumes)
            .Style(LabelColor, "EnvFiles:", NameLength)
            .Style(ValueColor, service.EnvFiles is null ? Null : string.Join("; ", service.EnvFiles)).LineBreak()
            .Style(LabelColor, "EnvVars:").LineBreak().AddConfigEnvVars(service.EnvVars)
            .Style(LabelColor, "Networks:").LineBreak().AddServiceNetworks(service.Networks)
            .Style(LabelColor, "Links:", NameLength)
            .Style(ValueColor, service.Links is null ? Null : string.Join("; ", service.Links)).LineBreak()
            .Style(LabelColor, "Hostname:", NameLength).Style(ValueColor, service.Hostname ?? Null).LineBreak()
            .Style(LabelColor, "ExtraHosts:").LineBreak().AddConfigExtraHosts(service.ExtraHosts)
            .Style(LabelColor, "Dns:", NameLength)
            .Style(ValueColor, service.Dns is null ? Null : string.Join("; ", service.Dns)).LineBreak()
            .Style(LabelColor, "Restart:", NameLength).Style(ValueColor, service.Restart ?? Null).LineBreak()
            .Style(LabelColor, "Entrypoint:", NameLength).Style(ValueColor, service.Entrypoint ?? Null).LineBreak()
            .Style(LabelColor, "Command:", NameLength)
            .Style(ValueColor, service.Command is null ? Null : string.Join(" ", service.Command)).LineBreak()
            .Style(LabelColor, "Expose:", NameLength)
            .Style(ValueColor, service.Expose is null ? Null : string.Join("; ", service.Expose)).LineBreak()
            .Style(LabelColor, "Attach:", NameLength).Style(ValueColor, service.Attach.ToString()).LineBreak()
            .Style(LabelColor, "RunCommandOptions:", NameLength)
            .Style(ValueColor, service.RunCommandOptions is null ? Null : string.Join(" ", service.RunCommandOptions)).LineBreak();

        return composer;
    }
    
    internal static CustomComposer AddNetworkConfig(this CustomComposer composer, NetworkConfig network)
    {
        
        return composer;
    }
    
    internal static CustomComposer AddServiceNetworks(this CustomComposer composer, Dictionary<string, ServiceNetworkConfig>? networks)
    {
        if (networks is null || networks.Count == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach ((string name, ServiceNetworkConfig network) in networks)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
            //     .Style(SpecialValueColor, $"{(string.IsNullOrEmpty(network.HostIp) ? "" : $":{network.HostIp}")}{network.Published ?? ""}")
            //     .Text(MappingSeparator).Style(ValueColor, $"{network.Target}{(string.IsNullOrEmpty(network.Protocol) ? "" : $"/{network.Protocol}")}")
            .LineBreak();
            
            // .Repeat(SubValuePrefix, SubValueIndent)
            // .Style(LabelColor, "Name:", NameLength - SubValueIndent).Style(ValueColor, service.Network?.Name ?? Null).LineBreak()
            // .Repeat(SubValuePrefix, SubValueIndent)
            // .Style(LabelColor, "Subnet:", NameLength - SubValueIndent).Style(ValueColor, service.Network?.Subnet ?? Null).LineBreak()
            // .Repeat(SubValuePrefix, SubValueIndent)
            // .Style(LabelColor, "IpRange:", NameLength - SubValueIndent).Style(ValueColor, service.Network?.IpRange ?? Null).LineBreak()
            // .Repeat(SubValuePrefix, SubValueIndent)
            // .Style(LabelColor, "IpAddress:", NameLength - SubValueIndent).Style(ValueColor, service.Network?.IpAddress ?? Null).LineBreak()
            // .Repeat(SubValuePrefix, SubValueIndent)
            // .Style(LabelColor, "Hostname:", NameLength - SubValueIndent).Style(ValueColor, service.Network?.Hostname ?? Null).LineBreak()
            // .Repeat(SubValuePrefix, SubValueIndent)
            // .Style(LabelColor, "Alias:", NameLength - SubValueIndent).Style(ValueColor, service.Network?.Alias ?? Null).LineBreak()
        }

        return composer;
    }

    internal static CustomComposer AddConfigPorts(this CustomComposer composer, PortMapping[]? ports)
    {
        if (ports is null || ports.Length == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach (var port in ports)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(SpecialValueColor, $"{(string.IsNullOrEmpty(port.HostIp) ? "" : $":{port.HostIp}")}{port.Published ?? ""}")
                .Text(MappingSeparator).Style(ValueColor, $"{port.Target}{(string.IsNullOrEmpty(port.Protocol) ? "" : $"/{port.Protocol}")}")
                .LineBreak();
        }

        return composer;
    }
    
    internal static CustomComposer AddConfigVolumes(this CustomComposer composer, VolumeConfig[]? volumes)
    {
        if (volumes is null || volumes.Length == 0)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(ValueColor, "[None]").LineBreak();
            return composer;
        }
        
        foreach (var volume in volumes)
        {
            composer.Repeat(SubValuePrefix, NameLength - 1).Space()
                .Style(SpecialColor, $"[{volume.Type}]").Space()
                .Style(SpecialValueColor, volume.Source)
                .Text(MappingSeparator).Style(ValueColor, volume.Target).Text(" (")
                .Style(LabelColor,"ReadOnly:").Space().Style(ValueColor, volume.ReadOnly ? "Yes" : "No").Text(" | ")
                .Style(LabelColor,"Bind.CreateHostPath:").Space().Style(ValueColor, volume.Bind?.CreateHostPath ?? false ? "Yes" : "No").Text(" | ")
                .Style(LabelColor,"Volume.NoCopy:").Space().Style(ValueColor, volume.Volume?.NoCopy ?? false ? "Yes" : "No")
                .Text(")").LineBreak();
        }

        return composer;
    }
    
    internal static CustomComposer AddConfigEnvVars(this CustomComposer composer, Dictionary<string, string>? envVars)
    {
        if (envVars is null || envVars.Count == 0)
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
    
    internal static CustomComposer AddConfigExtraHosts(this CustomComposer composer, Dictionary<string, string>? extraHosts)
    {
        if (extraHosts is null || extraHosts.Count == 0)
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