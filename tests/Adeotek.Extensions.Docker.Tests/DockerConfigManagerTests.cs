using System.Reflection;

namespace Adeotek.Extensions.Docker.Tests;

public class DockerConfigManagerTests
{
    private readonly string _fixturesPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
        "Fixtures");
        
    [Theory]
    [InlineData("config-full.json")]
    [InlineData("config-full.yml")]
    [InlineData("config-med.json")]
    [InlineData("config-med.yaml")]
    [InlineData("config-min.json")]
    [InlineData("config-min.yml")]
    public void LoadContainerConfig_WithValidFile_ReturnValidObject(string fixtureFile)
    {
        var configFile = Path.Combine(_fixturesPath, fixtureFile);

        var result = DockerConfigManager.LoadContainerConfig(configFile);
        
        Assert.NotNull(result.FullImageName);
        Assert.NotEqual(string.Empty, result.FullImageName.Trim());
        Assert.NotNull(result.CurrentName);
        Assert.NotEqual(string.Empty, result.CurrentName.Trim());
    }
    
    [Theory]
    [InlineData("yaml", "yml")]
    [InlineData("json", "json")]
    public void GetSerializedSampleConfig_ReturnValidString(string format, string fixtureExtension)
    {
        var configFile = Path.Combine(_fixturesPath, $"config-full.{fixtureExtension}");
        var expectedResult = File.ReadAllText(configFile).Trim();

        var result = DockerConfigManager.GetSerializedSampleConfig(format);
        
        Assert.Equal(expectedResult, result.Trim());
    }
}