using Adeotek.Extensions.Docker.Config;

namespace Adeotek.Extensions.Docker.Tests;

public class DockerCliCommandTests
{
    private readonly DockerCliCommand _sut;
    
    public DockerCliCommandTests()
    {
        var provider = TestHelpers.GetShellProcessProvider(out _);
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
    public void AddRestartArg_WithNonNullValue_SetArgsDictValue()
    {
        var expectedValue = "--restart=always";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddRestartArg("always");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddRestartArg_WithNonNullValue_DoNotSetAnything()
    {
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddRestartArg("");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.True(_sut.Args.Length == 1);
        Assert.Equal("--some-argument", _sut.Args[0]);
    }
    
    [Fact]
    public void AddRestartArg_WithNullValue_SetArgsDictValue()
    {
        var expectedValue = "--restart=unless-stopped";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddRestartArg(null);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddPullPolicyArg_WithNonNullValue_SetArgsDictValue()
    {
        var expectedValue = "--pull=always";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPullPolicyArg("always");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddPullPolicyArg_WithNonNullValue_DoNotSetAnything()
    {
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPullPolicyArg("");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.True(_sut.Args.Length == 1);
        Assert.Equal("--some-argument", _sut.Args[0]);
    }
    
    [Fact]
    public void AddPullPolicyArg_WithNullValue_SetArgsDictValue()
    {
        var expectedValue = "--pull=missing";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPullPolicyArg(null);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddDockerCommandOptionsArgs_WithNoOptions_SetArgsDefaultOption()
    {
        var expectedValue = "-d";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddDockerCommandOptionsArgs(Array.Empty<string>());
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddDockerCommandOptionsArgs_WithTwoOptions_SetOptionsArgs()
    {
        var expectedValue = "-it -m 128";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddDockerCommandOptionsArgs(new [] { "-it", "-m 128" });
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddDockerCommandOptionsArgs_WithNoOptionsAndNotIsRun_DoNotSetArgs()
    {
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddDockerCommandOptionsArgs(Array.Empty<string>(), false);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Single(_sut.Args);
    }
    
    [Fact]
    public void AddDockerCommandOptionsArgs_WithTwoOptionsAndNotIsRun_SetOptionsArgs()
    {
        var expectedValue = "-it -m 128";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddDockerCommandOptionsArgs(new [] { "-it", "-d", "-m 128" }, false);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddStartupCommandArgs_WithEmptyCommand_DoNotSetCommandArgs()
    {
        ServiceConfig config = new()
        {
            Command = null
        };

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddStartupCommandArgs(config);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Single(_sut.Args);
    }
    
    [Fact]
    public void AddStartupCommandArgs_WithTwoOptions_SetOptionsArgs()
    {
        ServiceConfig config = new()
        {
            Command = new [] { "serve -v", "--debug" }
        };
        var expectedValue = "serve -v --debug";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddStartupCommandArgs(config);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(2, _sut.Args.Length);
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddPortArg_WithMinimalArguments_SetArgsDictValue()
    {
        var expectedValue = "-p 1234:9876";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPortArg("1234", "9876");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddPortArg_WithAllArguments_SetArgsDictValue()
    {
        var expectedValue = "-p 1.2.3.4:1234:9876/tcp";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPortArg("1234", "9876", "1.2.3.4", "tcp");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddPortsArgs_SetArgsDictValues()
    {
        var expectedFirstValue = "-p 1234:9876";
        var expectedLastValue = "-p 443";
        var ports = new PortMapping[]
        {
            new() { Published = "1234", Target = "9876" }, 
            new() { Published = "80", Target = "80" }, 
            new() { Target = "443" },
        };

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddPortsArgs(ports);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(4, _sut.Args.Length);
        Assert.Equal(expectedFirstValue, _sut.Args[1]);
        Assert.Equal(expectedLastValue, _sut.Args[3]);
    }
    
    [Fact]
    public void AddExposedPortArgs_WithNotEmptyArgument_SetArgsDictValue()
    {
        var expectedValue = "--expose 5555-6666";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddExposedPortArgs("5555-6666");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddExposedPortsArgs_SetArgsDictValues()
    {
        var expectedFirstValue = "--expose 443";
        var expectedLastValue = "--expose 5555-6666";
        var ports = new [] { "443", "5555-6666" };

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddExposedPortsArgs(ports);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(3, _sut.Args.Length);
        Assert.Equal(expectedFirstValue, _sut.Args[1]);
        Assert.Equal(expectedLastValue, _sut.Args[2]);
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
            new() { Source = "volume-name", Target = "/path/in/container", ReadOnly = false }, 
            new() { Type = "bind", Source = "/some/path/on/host", Target = "/another/path/in/container", ReadOnly = true }
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
    public void AddServiceNetworkArgs_WithNotNullServiceNetwork_SetServiceNetworkArgs()
    {
        var expectedIpArg = "--ip=10.2.3.4";
        var expectedFirstNetworkAliasArg = "--network-alias=alias-host-name";
        var expectedSecondNetworkAliasArg = "--network-alias=other-host-name";
        ServiceNetworkConfig serviceNetworkConfig = new()
        {
            IpV4Address = "10.2.3.4", 
            Aliases = new[] { "alias-host-name", "other-host-name" }
        };
        
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddServiceNetworkArgs(serviceNetworkConfig);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(4, _sut.Args.Length);
        Assert.Equal(expectedIpArg, _sut.Args[1]);
        Assert.Equal(expectedFirstNetworkAliasArg, _sut.Args[2]);
        Assert.Equal(expectedSecondNetworkAliasArg, _sut.Args[3]);
    }
    
    [Fact]
    public void AddServiceNetworkArgs_WithNullNetwork_DoNotSetServiceNetworkArgs()
    {
        ServiceNetworkConfig? serviceNetworkConfig = null;
        
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddServiceNetworkArgs(serviceNetworkConfig);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Single(_sut.Args);
    }

    [Fact]
    public void AddDefaultNetworkArgs_WithNotNullNetwork_SetArgsDictValues()
    {
        var expectedNetworkArg = "--network=docker-test-network";
        var expectedHostnameArg = "--hostname=some-host-name";
        var expectedIpArg = "--ip=10.2.3.4";
        var expectedFirstNetworkAliasArg = "--network-alias=alias-host-name";
        var expectedSecondNetworkAliasArg = "--network-alias=other-host-name";
        ServiceConfig config = new()
        {
            Networks = new Dictionary<string, ServiceNetworkConfig?>
            {
                {
                    "test-network",
                    new ServiceNetworkConfig
                    {
                        IpV4Address = "10.2.3.4",
                        Aliases = new []
                        {
                            "alias-host-name",
                            "other-host-name"
                        }
                    }
                }
            },
            Hostname = "some-host-name"
        };
        List<NetworkConfig> networks = new()
        {
            new NetworkConfig { Name = "docker-test-network" }.SetNetworkName("test-network")
        };
        
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddDefaultNetworkArgs(config, networks);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(6, _sut.Args.Length);
        Assert.Equal(expectedNetworkArg, _sut.Args[1]);
        Assert.Equal(expectedHostnameArg, _sut.Args[2]);
        Assert.Equal(expectedIpArg, _sut.Args[3]);
        Assert.Equal(expectedFirstNetworkAliasArg, _sut.Args[4]);
        Assert.Equal(expectedSecondNetworkAliasArg, _sut.Args[5]);
    }
    
    [Fact]
    public void AddDefaultNetworkArgs_WithNullNetwork_SetArgsDictValues()
    {
        ServiceConfig config = new()
        {
            Networks = null
        };
        
        List<NetworkConfig> networks = new()
        {
            new NetworkConfig { Name = "docker-test-network" }.SetNetworkName("test-network")
        };
        
        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddDefaultNetworkArgs(config, networks);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Single(_sut.Args);
    }
    
    [Fact]
    public void AddExtraHostArg_WithHostGateway_SetArgsDictValue()
    {
        var expectedValue = "--add-host host.docker.internal:host-gateway";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddExtraHostArg("host.docker.internal", "host-gateway");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddExtraHostArg_WithIp_SetArgsDictValue()
    {
        var expectedValue = "--add-host some.host.name:1.2.3.4";

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddExtraHostArg("some.host.name", "1.2.3.4");
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue, _sut.Args[1]);
    }
    
    [Fact]
    public void AddExtraHostsArgs_SetArgsDictValues()
    {
        var expectedValue1 = "--add-host host.docker.internal:host-gateway";
        var expectedValue2 = "--add-host some.host.name:1.2.3.4";
        var expectedValue3 = "--add-host another-container:172.10.1.123";
        var envArgs = new Dictionary<string, string>
        {
            { "host.docker.internal", "host-gateway" }, 
            { "some.host.name", "1.2.3.4" }, 
            { "another-container", "172.10.1.123" },
        };

        var result = _sut.ClearArgs()
            .AddArg("--some-argument")
            .AddExtraHostsArgs(envArgs);
        
        Assert.Equal(typeof(DockerCliCommand), result.GetType());
        Assert.Equal(expectedValue1, _sut.Args[1]);
        Assert.Equal(expectedValue2, _sut.Args[2]);
        Assert.Equal(expectedValue3, _sut.Args[3]);
        Assert.Equal(4, _sut.Args.Length);
    }
}