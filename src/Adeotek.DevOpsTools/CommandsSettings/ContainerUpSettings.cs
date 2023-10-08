using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

internal sealed class ContainerUpSettings : ContainerSettings
{
    [Description("Upgrade container if it exists")]
    [CommandOption("-u|--upgrade")]
    [DefaultValue(false)]
    public bool Upgrade { get; init; }
    
    [Description("Replace existing container, instead of demoting it to 'backup'\nTo be used together with the '--update' option")]
    [CommandOption("-r|--replace")]
    [DefaultValue(false)]
    public bool Replace { get; init; }
    
    [Description("Force container recreation (update), even if the container is on the latest image\nTo be used together with the '--update' option")]
    [CommandOption("-f|--force")]
    [DefaultValue(false)]
    public bool Force { get; init; }
}
