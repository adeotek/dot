using Adeotek.DevOpsTools.Commands;
using Adeotek.DevOpsTools.Common;

using Spectre.Console;
using Spectre.Console.Cli;

CommandApp app = new();
app.Configure(config =>
{
    config.SetApplicationName("act"); // ???
    config.SetHelpProvider(new DefaultHelpProvider(config.Settings));
    config.SetExceptionHandler(e =>
    {
        AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        return 1;
    });
    // Commands
    config.AddCommand<DockerContainerCommand>("container")
        .WithAlias("ct");
    config.AddCommand<TestCommand>("test");
});
return app.Run(args);