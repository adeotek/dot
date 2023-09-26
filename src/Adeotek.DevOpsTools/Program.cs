﻿using Adeotek.DevOpsTools.Commands;
using Adeotek.DevOpsTools.Common;

using Spectre.Console;
using Spectre.Console.Cli;

CommandApp app = new();
app.Configure(config =>
{
    config.SetApplicationName("dot");
    config.SetHelpProvider(new DefaultHelpProvider(config.Settings));
    config.SetExceptionHandler(e =>
    {
        AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        return 1;
    });
    // Commands
    config.AddBranch("container", ct =>
    {
        ct.SetDescription("Manage Docker containers.");
        ct.AddCommand<ContainerUpCommand>("up")
            .WithDescription("Create/Update Docker containers");
        ct.AddCommand<ContainerDownCommand>("down")
            .WithDescription("Remove Docker containers");
    });
});
return app.Run(args);