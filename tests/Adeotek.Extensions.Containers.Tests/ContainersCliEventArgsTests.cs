namespace Adeotek.Extensions.Containers.Tests;

public class ContainersCliEventArgsTests
{
    private readonly Dictionary<string, string?> _testData = new()
    {
        { "key1", "value1" }, 
        { "key2", null }, 
        { "key3", "some other value" }, 
        { "key4", "" }
    };
    
    [Fact]
    public void DataToString_WithoutKeys_ReturnValidString()
    {
        var expectedString = "value1 | ?? | some other value | ";
        ContainersCliEventArgs eventArgs = new(_testData, ContainersCliEventType.Message);

        var result = eventArgs.DataToString(" | ", "??");
        
        Assert.Equal(expectedString, result);
    }
    
    [Fact]
    public void DataToString_WithKeys_ReturnValidString()
    {
        var expectedString = "[key1] -> value1 | [key2] -> ?? | [key3] -> some other value | [key4] -> ";
        ContainersCliEventArgs eventArgs = new(_testData, ContainersCliEventType.Message);

        var result = eventArgs.DataToString(" | ", "??", false);
        
        Assert.Equal(expectedString, result);
    }
}