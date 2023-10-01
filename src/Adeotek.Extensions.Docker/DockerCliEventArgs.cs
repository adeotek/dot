using System.Collections.ObjectModel;
using System.Text;

namespace Adeotek.Extensions.Docker;

public enum DockerCliEventType
{
    Message = 0,
    Command = 1,
    StdOutput = 2,
    ErrOutput = 3,
    ExitCode = 4
}

public class DockerCliEventArgs : EventArgs
{
    public DockerCliEventType Type { get; }
    public ReadOnlyDictionary<string, string?> Data { get; }
    
    public DockerCliEventArgs(Dictionary<string, string?> data, DockerCliEventType type)
    {
        Type = type;
        Data = new ReadOnlyDictionary<string, string?>(data);
    }
    
    public DockerCliEventArgs(string? data, DockerCliEventType type)
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