using System.Reflection;

namespace Adeotek.Extensions.Containers.Tests;

public class ContainersConfigManagerTests
{
    private readonly string _fixturesPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
        "Fixtures");
        
    [Theory]
    [InlineData("config-full.json", 2, 2)]
    [InlineData("config-full.yml", 2, 2)]
    [InlineData("config-med.json", 1, 1)]
    [InlineData("config-med.yaml", 1, 1)]
    [InlineData("config-min.json", 1, 0)]
    [InlineData("config-min.yml", 1, 0)]
    public void LoadContainerConfig_WithValidFile_ReturnValidObject(string fixtureFile, 
        int servicesCount, int networksCount)
    {
        var configFile = Path.Combine(_fixturesPath, fixtureFile);

        var result = ContainersConfigManager.LoadContainersConfig(configFile);
        
        Assert.Equal(servicesCount, result.Services.Count);
        Assert.Equal(networksCount, result.Networks.Count);
        Assert.False(string.IsNullOrWhiteSpace(result.Services.First().Value.Image));
        Assert.False(string.IsNullOrWhiteSpace(result.Services.First().Value.CurrentName));
    }
    
    [Theory]
    [InlineData("yaml", "yml")]
    [InlineData("json", "json")]
    public void GetSerializedSampleConfig_ReturnValidString(string format, string fixtureExtension)
    {
        var configFile = Path.Combine(_fixturesPath, $"config-full.{fixtureExtension}");
        var expectedResult = File.ReadAllText(configFile).Trim();

        var result = ContainersConfigManager.GetSerializedSampleConfig(format);
        
        Assert.Equal(expectedResult.Trim(), result.Trim());
    }
}