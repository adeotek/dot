using Adeotek.DevOpsTools.Commands;
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
    
    config.AddBranch("container", ct =>
    {
        ct.SetDescription("Manage Docker containers");
        ct.AddCommand<ContainerUpCommand>("up")
            .WithDescription("Create/Update Docker containers");
        ct.AddCommand<ContainerDownCommand>("down")
            .WithDescription("Remove Docker containers");
        ct.AddBranch("config", cfg =>
        {
            cfg.SetDescription("Validate/Generate Docker containers config files");
            cfg.AddCommand<ContainerConfigValidateCommand>("validate")
                .WithDescription("Validate Docker container config file");
            cfg.AddCommand<ContainerConfigSampleCommand>("sample")
                .WithDescription("Generate Docker container config sample file");
        });
    });
    
    config.AddBranch("utf8bom", bom =>
    {
        bom.SetDescription("Add/Remove/Check BOM (Byte Order Mark) signature of UTF-8 encoded files");
        bom.AddCommand<Utf8BomAddCommand>("add")
            .WithDescription("Add UTF8 Signature (BOM) to files");
        bom.AddCommand<Utf8BomRemoveCommand>("remove")
            .WithDescription("Add UTF8 Signature (BOM) from files");
    });
});
return app.Run(args);