namespace Adeotek.Extensions.Processes.Tests;

public class ShellCommandTests
{
    [Fact]
    public void Prepare_WithBashCommand_ExpectValidProcessArgs()
    {
        var expectedArgs = "-c \"ls -lA\"";
        ShellCommand command = new(ShellCommand.BashShell)
        {
            IsScript = false, 
            Command = "ls"
        };
        command.AddArg("-lA");

        command.Prepare();
        
        Assert.Equal(ShellCommand.BashShell, command.ProcessFile);
        Assert.Equal(expectedArgs, command.ProcessArguments);
    }
    
    [Fact]
    public void Prepare_WithBashScript_ExpectValidProcessArgs()
    {
        var expectedArgs = "./some_script.sh --some-arg abc";
        ShellCommand command = new(ShellCommand.BashShell)
        {
            IsScript = true, 
            Command = "./some_script.sh"
        };
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
        ShellCommand command = new()
        {
            Shell = ShellCommand.PsShell, 
            IsScript = false, 
            Command = "Get-ChildItem"
        };
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
        ShellCommand command = new()
        {
            Shell = ShellCommand.PsShell, 
            IsScript = true, 
            Command = ".\\some_script.ps1"
        };
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
        ShellCommand command = new()
        {
            IsScript = false, 
            Command = commandName
        };
        command.AddArg("ps");
        command.AddArg("--all");

        command.Prepare();
        
        Assert.Equal(commandName, command.ProcessFile);
        Assert.Equal(expectedArgs, command.ProcessArguments);
    }
}