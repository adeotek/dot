using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings.Containers;

internal sealed class ContainersBackupSettings : ContainersSettings
{
    [Description("Volumes backup location (absolute/relative path)")]
    [CommandArgument(1, "<backup_location>")]
    public string? BackupLocation { get; init; }
    
    [Description("Execute the command only for the service with the provided name")]
    [CommandOption("-n|--name <value>")]
    public string? ServiceName { get; init; }
    
    [Description("Execute the command only for the volume with the provided name/source path\nOnly works together with the '--name' option")]
    [CommandOption("-t|--target <value>")]
    public string? TargetVolume { get; init; }
}
