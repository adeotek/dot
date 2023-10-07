using Adeotek.Extensions.Docker.Config;
using Adeotek.Extensions.Processes;

namespace Adeotek.Extensions.Docker.Tests;

public class DockerCliCommandTests
{
    private readonly IShellProcess _shellProcessMock;
    private readonly DockerCliCommand _sut;

    public DockerCliCommandTests()
    {
        var provider = TestHelpers.GetShellProcessProvider(out _shellProcessMock);
        _sut = new DockerCliCommand(provider) { Command = "docker" };
    }

    [Fact]
    public void AddFilterArg_WithKey_SetArgsDictValue()
    {
        var expectedValue = "--filter f-key=f-value";

        var result = _sut.ClearArgs()
            .AddArg("some-command")
            .AddFilterArg("f-value", "f-key");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddFilterArg_WithoutKey_SetArgsDictValue()
    {
        var expectedValue = "--filter name=f-value";

        var result = _sut.ClearArgs()
            .AddArg("some-command")
            .AddFilterArg("f-value");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddFilterArg_WithNonNullValue_SetArgsDictValue()
    {
        var expectedValue = "--restart=always";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddRestartArg("always");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddFilterArg_WithNullValue_SetArgsDictValue()
    {
        var expectedValue = "--restart=unless-stopped";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddRestartArg(null);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddPortArg_SetArgsDictValue()
    {
        var expectedValue = "-p 1234:9876";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPortArg(1234, 9876);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddPortsArgs_SetArgsDictValues()
    {
        var expectedValue = "-p 1234:9876";
        var ports = new PortMapping[]
        {
            new() { Host = 1234, Container = 9876 }, 
            new() { Host = 80, Container = 80 }, 
            new() { Host = 443, Container = 443 },
        };

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPortsArgs(ports);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
        Assert.Equal(4, _sut.Args.Length);
    }
    
    [Fact]
    public void AddVolumeArg_WithReadonly_SetArgsDictValue()
    {
        var expectedValue = "-v volume-name:/path/in/container:ro";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddVolumeArg("volume-name", "/path/in/container", true);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddVolumeArg_WithoutReadonly_SetArgsDictValue()
    {
        var expectedValue = "-v volume-name:/path/in/container";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddVolumeArg("volume-name", "/path/in/container");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddVolumesArgs_SetArgsDictValues()
    {
        var expectedFirstValue = "-v volume-name:/path/in/container";
        var expectedSecondValue = "-v /some/path/on/host:/another/path/in/container:ro";
        var volumes = new VolumeConfig[]
        {
            new() { Source = "volume-name", Destination = "/path/in/container", IsReadonly = false }, 
            new() { Source = "/some/path/on/host", Destination = "/another/path/in/container", IsReadonly = true },
        };

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddVolumesArgs(volumes);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedFirstValue, _sut.Args[1]);
        Assert.Equal(expectedSecondValue, _sut.Args[2]);
        Assert.Equal(3, _sut.Args.Length);
    }
    
    [Fact]
    public void AddEnvVarArg_WithoutEqualOrSpace_SetArgsDictValue()
    {
        var expectedValue = "-e ENV_VAR_NAME=someValue";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddEnvVarArg("ENV_VAR_NAME", "someValue");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddEnvVarArg_WithEqual_SetArgsDictValue()
    {
        var expectedValue = "-e ENV_VAR_NAME=\"some=value\"";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddEnvVarArg("ENV_VAR_NAME", "some=value");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddEnvVarArg_WithSpace_SetArgsDictValue()
    {
        var expectedValue = "-e ENV_VAR_NAME=\"some value\"";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddEnvVarArg("ENV_VAR_NAME", "some value");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    //AddEnvVarsArgs
    [Fact]
    public void AddEnvVarsArgs_SetArgsDictValues()
    {
        var expectedValue1 = "-e ENV_VAR_1=someValue";
        var expectedValue2 = "-e ENV_VAR_2=\"some value\"";
        var expectedValue3 = "-e ENV_VAR_3=\"some=value\"";
        var envArgs = new Dictionary<string, string>
        {
            { "ENV_VAR_1", "someValue" }, 
            { "ENV_VAR_2", "some value" }, 
            { "ENV_VAR_3", "some=value" },
        };

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddEnvVarsArgs(envArgs);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue1, _sut.Args[1]);
        Assert.Equal(expectedValue2, _sut.Args[2]);
        Assert.Equal(expectedValue3, _sut.Args[3]);
        Assert.Equal(4, _sut.Args.Length);
    }

    [Fact]
    public void AddNetworkArgs_WithNotNullNetwork_SetArgsDictValues()
    {
        var expectedNetworkArg = "--network=test-network";
        var expectedIpArg = "--ip=10.2.3.4";
        var expectedHostnameArg = "--hostname=some-host-name";
        var expectedNetworkAliasArg = "--network-alias=other-host-name";
        ContainerConfig config = new()
        {
            Image = "SomeImage",
            BaseName = "some-name",
            Network = new NetworkConfig
            {
                Name = "test-network",
                IpAddress = "10.2.3.4",
                Hostname = "some-host-name",
                Alias = "other-host-name"
            }
        };
        
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddNetworkArgs(config);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedNetworkArg, _sut.Args[1]);
        Assert.Equal(expectedIpArg, _sut.Args[2]);
        Assert.Equal(expectedHostnameArg, _sut.Args[3]);
        Assert.Equal(expectedNetworkAliasArg, _sut.Args[4]);
        Assert.Equal(5, _sut.Args.Length);
    }
    
    [Fact]
    public void AddNetworkArgs_WithNullNetwork_SetArgsDictValues()
    {
        ContainerConfig config = new()
        {
            Image = "SomeImage",
            BaseName = "some-name",
            Network = null
        };
        
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddNetworkArgs(config);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Single(_sut.Args);
    }
}