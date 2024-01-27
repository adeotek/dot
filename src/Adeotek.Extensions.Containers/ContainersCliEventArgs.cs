using System.Collections.ObjectModel;
using System.Text;

namespace Adeotek.Extensions.Containers;

public enum ContainersCliEventType
{
    Message = 0,
    Command = 1,
    StdOutput = 2,
    ErrOutput = 3,
    ExitCode = 4
}

public class ContainersCliEventArgs : EventArgs
{
    public ContainersCliEventType Type { get; }
    public ReadOnlyDictionary<string, string?> Data { get; }
    
    public ContainersCliEventArgs(Dictionary<string, string?> data, ContainersCliEventType type)
    {
        Type = type;
        Data = new ReadOnlyDictionary<string, string?>(data);
    }
    
    public ContainersCliEventArgs(string? data, ContainersCliEventType type)
    {
        Type = type;
        Data = new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>{ { type.ToString(), data } });
    }

    public string DataToString(string separator, string nullValue = "", bool ignoreKeys = true)
    {
        StringBuilder sb = new();
        string? sep = null;
        foreach ((string key, string? value) in Data)
        {
            if (ignoreKeys)
            {
                sb.Append($"{sep}{value ?? nullValue}");
            }
            else
            {
                sb.Append($"{sep}[{key}] -> {value ?? nullValue}");
            }

            sep ??= separator;
        }
        return sb.ToString();
    }
}