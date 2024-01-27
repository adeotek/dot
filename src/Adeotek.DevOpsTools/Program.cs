using Adeotek.DevOpsTools.Commands.Containers;
using Adeotek.DevOpsTools.Commands.Email;
using Adeotek.DevOpsTools.Commands.Files;
using Adeotek.DevOpsTools.Commands.Networking;
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

    configurator.AddBranch("container", containers =>
    {
        containers.SetDescription("Manage Docker/Podman containers (deprecated)");
        containers.AddCommand<ContainersUpCommand>("up")
            .WithDescription("Create/Update Docker/Podman containers")
            .WithData("v1");
        containers.AddCommand<ContainersDownCommand>("down")
            .WithDescription("Remove Docker/Podman containers")
            .WithData("v1");
        containers.AddCommand<ContainersActionCommand>("start")
            .WithDescription("Start Docker/Podman containers")
            .WithData("v1");
        containers.AddCommand<ContainersActionCommand>("stop")
            .WithDescription("Stop Docker/Podman containers")
            .WithData("v1");
        containers.AddCommand<ContainersActionCommand>("restart")
            .WithDescription("Restart Docker/Podman containers")
            .WithData("v1");
        containers.AddBranch("config", config =>
        {
            config.SetDescription("Validate/Generate Docker/Podman containers config files");
            config.AddCommand<ContainersConfigValidateCommand>("validate")
                .WithDescription("Validate Docker/Podman container config file")
                .WithData("v1");
            config.AddCommand<ContainersConfigSampleCommand>("sample")
                .WithDescription("Generate Docker/Podman container config sample file")
                .WithData("v1");
        });
    });

    configurator.AddBranch("containers", containers =>
    {
        containers.SetDescription("Manage Docker/Podman containers");
        containers.AddCommand<ContainersUpCommand>("up")
            .WithDescription("Create/Update Docker/Podman containers");
        containers.AddCommand<ContainersDownCommand>("down")
            .WithDescription("Remove Docker/Podman containers");
        containers.AddCommand<ContainersBackupCommand>("backup")
            .WithDescription("Backup Docker/Podman container volumes");
        containers.AddCommand<ContainersActionCommand>("start")
            .WithDescription("Start Docker/Podman containers");
        containers.AddCommand<ContainersActionCommand>("stop")
            .WithDescription("Stop Docker/Podman containers");
        containers.AddCommand<ContainersActionCommand>("restart")
            .WithDescription("Restart Docker/Podman containers");
        containers.AddBranch("config", config =>
        {
            config.SetDescription("Validate/Generate Docker/Podman containers config files");
            config.AddCommand<ContainersConfigValidateCommand>("validate")
                .WithDescription("Validate Docker/Podman container config file");
            config.AddCommand<ContainersConfigSampleCommand>("sample")
                .WithDescription("Generate Docker/Podman container config sample file");
        });
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
        port.AddCommand<PortProbeCommand>("probe")
            .WithDescription("Probe (check if is listening) a local or remote TCP port");
    });

    configurator.AddBranch("utf8bom", utf8Bom =>
    {
        utf8Bom.SetDescription("Add/Remove/Check BOM (Byte Order Mark) signature of UTF-8 encoded files");
        utf8Bom.AddCommand<Utf8BomAddCommand>("add")
            .WithDescription("Add UTF8 Signature (BOM) to files");
        utf8Bom.AddCommand<Utf8BomRemoveCommand>("remove")
            .WithDescription("Add UTF8 Signature (BOM) from files");
    });
});
return app.Run(args);