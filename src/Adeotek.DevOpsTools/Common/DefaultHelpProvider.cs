using System.Reflection;

using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace Adeotek.DevOpsTools.Common;

internal sealed class DefaultHelpProvider : HelpProvider
{
    private const string ApplicationTitle = "AdeoTEK DevOps Tools";
    private const char HS = '-';
    private const char HT = '-';
    private const int HTL = 12;
    private readonly string _applicationVersion;

    public DefaultHelpProvider(ICommandAppSettings settings) : base(settings)
    {
        _applicationVersion = settings.ApplicationVersion
                              ?? Assembly.GetEntryAssembly()
                                  ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                  ?.InformationalVersion.Split("+")[0]
                              ?? "?";
    }

    public override IEnumerable<IRenderable> GetHeader(ICommandModel model, ICommandInfo? command)
    {
        double spaces = Math.Max(0, 
            (ApplicationTitle.Length - model.ApplicationName.Length - _applicationVersion.Length) / 2d);
        var composer = new CustomComposer()
            // Top separator row
            .Repeat(HS, (2 * HTL) + 2 + ApplicationTitle.Length).LineBreak()
            // Application title row
            .Repeat(HT, HTL).Space().Style("darkorange", ApplicationTitle)
            .Space().Repeat(HT, HTL).LineBreak()
            // Application version row
            .Repeat(HT, HTL)
            .Spaces((int)Math.Ceiling(spaces)).Style("purple", model.ApplicationName)
            .Space().Style("green", $"v{_applicationVersion}").Spaces((int)Math.Floor(spaces))
            .Repeat(HT, HTL).LineBreak()
            // Bottom separator row
            .Repeat(HS, (2 * HTL) + 2 + ApplicationTitle.Length).LineBreak()
            // Final line break
            .LineBreak();
        return new[] { composer };
    }
}