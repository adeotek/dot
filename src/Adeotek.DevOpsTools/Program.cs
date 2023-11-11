using Adeotek.DevOpsTools.Commands;
using Adeotek.DevOpsTools.Common;

using Spectre.Console;
using Spectre.Console.Cli;

CommandApp app = new();
app.Configure(configurator =>
{
    configurator.SetApplicationName("dot");
    configurator.SetHelpProvider(new DefaultHelpProvider(configurator.Settings));
    configurator.SetExceptionHandler(e =>
    {
        AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        return 1;
    });
    
    configurator.AddBranch("container", container =>
    {
        container.SetDescription("Manage Docker containers");
        container.AddCommand<ContainerUpCommand>("up")
            .WithDescription("Create/Update Docker containers");
        container.AddCommand<ContainerDownCommand>("down")
            .WithDescription("Remove Docker containers");
        container.AddBranch("config", config =>
        {
            config.SetDescription("Validate/Generate Docker containers config files");
            config.AddCommand<ContainerConfigValidateCommand>("validate")
                .WithDescription("Validate Docker container config file");
            config.AddCommand<ContainerConfigSampleCommand>("sample")
                .WithDescription("Generate Docker container config sample file");
        });
    });
    
    configurator.AddBranch("utf8bom", utf8Bom =>
    {
        utf8Bom.SetDescription("Add/Remove/Check BOM (Byte Order Mark) signature of UTF-8 encoded files");
        utf8Bom.AddCommand<Utf8BomAddCommand>("add")
            .WithDescription("Add UTF8 Signature (BOM) to files");
        utf8Bom.AddCommand<Utf8BomRemoveCommand>("remove")
            .WithDescription("Add UTF8 Signature (BOM) from files");
    });
    
    configurator.AddBranch("email", email =>
    {
        email.SetDescription("Email tools");
        email.AddCommand<EmailSendCommand>("send")
            .WithDescription("Send an email message based on a configuration file or provided options");
    });
    
    configurator.AddBranch("port", port =>
    {
        port.SetDescription("TCP ports testing");
        port.AddCommand<PortListenCommand>("listen")
            .WithDescription("Start a listener on the provided TCP port");
    });
});
return app.Run(args);
