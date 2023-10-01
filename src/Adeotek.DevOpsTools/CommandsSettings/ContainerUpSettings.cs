using System.ComponentModel;

using Spectre.Console.Cli;

namespace Adeotek.DevOpsTools.CommandsSettings;

internal sealed class ContainerUpSettings : ContainerSettings
{
    [Description("Update container if it exists")]
    [CommandOption("-u|--update")]
    [DefaultValue(false)]
    public bool Update { get; init; }
    
    [Description("Replace existing container, instead of demoting it to 'backup'\nTo be used together with the '--update' option")]
    [CommandOption("-r|--replace")]
    [DefaultValue(false)]
    public bool Replace { get; init; }
}
