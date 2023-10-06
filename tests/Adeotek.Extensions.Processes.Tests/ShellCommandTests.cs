namespace Adeotek.Extensions.Processes.Tests;

public class ShellCommandTests
{
    [Fact]
    public void Prepare_WithBashCommand_ExpectValidProcessArgs()
    {
        var expectedArgs = "-c \"ls -lA\"";
        var command = ShellCommand.GetShellCommandInstance(
            shell: ShellCommand.BashShell, 
            command: "ls", 
            isScript: false);
        command.AddArg("-lA");

        command.Prepare();
        
        Assert.Equal(ShellCommand.BashShell, command.ProcessFile);
        Assert.Equal(expectedArgs, command.ProcessArguments);
    }
    
    [Fact]
    public void Prepare_WithBashScript_ExpectValidProcessArgs()
    {
        var expectedArgs = "./some_script.sh --some-arg abc";
        var command = ShellCommand.GetShellCommandInstance(
            shell: ShellCommand.BashShell, 
            command: "./some_script.sh", 
            isScript: true);
        command.AddArg("--some-arg abc");

        command.Prepare();
        
        Assert.Equal(ShellCommand.BashShell, command.ProcessFile);
        Assert.Equal(expectedArgs, command.ProcessArguments);
    }
    
    [Fact]
    public void Prepare_WithPwshCommand_ExpectValidProcessArgs()
    {
        var expectedArgs = ShellCommand.IsWindowsPlatform
            ? "-NoProfile -c \"Get-ChildItem Env: | Select Name\""
            : "-c \"Get-ChildItem Env: | Select Name\"";
        var command = ShellCommand.GetShellCommandInstance(
            shell: ShellCommand.PsShell, 
            command: "Get-ChildItem", 
            isScript: false);
        command.AddArg("Env:");
        command.AddArg("| Select Name");

        command.Prepare();
        
        Assert.Equal(ShellCommand.PsShell, command.ProcessFile);
        Assert.Equal(expectedArgs, command.ProcessArguments);
    }
    
    [Fact]
    public void Prepare_WithPwshScript_ExpectValidProcessArgs()
    {
        var expectedArgs = ShellCommand.IsWindowsPlatform
            ? "-NoProfile .\\some_script.ps1 --some-arg abc"
            : ".\\some_script.ps1 --some-arg abc";
        var command = ShellCommand.GetShellCommandInstance(
            shell: ShellCommand.PsShell, 
            command: ".\\some_script.ps1", 
            isScript: true);
        command.AddArg("--some-arg abc");

        command.Prepare();
        
        Assert.Equal(ShellCommand.PsShell, command.ProcessFile);
        Assert.Equal(expectedArgs, command.ProcessArguments);
    }
    
    [Fact]
    public void Prepare_WithDockerCommand_ExpectValidProcessArgs()
    {
        var commandName = "docker";
        var expectedArgs = "ps --all";
        var command = ShellCommand.GetShellCommandInstance(command: commandName);
        command.AddArg("ps");
        command.AddArg("--all");

        command.Prepare();
        
        Assert.Equal(commandName, command.ProcessFile);
        Assert.Equal(expectedArgs, command.ProcessArguments);
    }
}