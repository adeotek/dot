using System.Diagnostics.CodeAnalysis;

namespace Adeotek.Extensions.Processes;

public delegate void OutputReceivedEventHandler(object sender, OutputReceivedEventArgs e);

[ExcludeFromCodeCoverage]
public class OutputReceivedEventArgs : EventArgs
{
    public string? Data { get; }
    public bool IsError { get; }
    
    public OutputReceivedEventArgs(string? data, bool isError = false)
    {
        Data = data;
        IsError = isError;
    }
}