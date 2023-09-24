using Adeotek.DevOpsTools.Commands;
using Adeotek.DevOpsTools.Common;

using Spectre.Console.Cli;

CommandApp app = new();
app.Configure(config =>
{
    config.SetApplicationName("act"); // ???
    config.SetHelpProvider(new DefaultHelpProvider(config.Settings));
    config.AddCommand<DockerContainerCommand>("container")
        .WithAlias("ct");
    config.AddCommand<TestCommand>("test");
});
return app.Run(args);