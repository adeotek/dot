namespace Adeotek.CommandLine.Helpers;

public class OutputReceivedEventArgs : EventArgs
{
    public OutputReceivedEventArgs(string? data, bool isError = false)
    {
        Data = data;
        IsError = isError;
    }

    public string? Data { get; }
    public bool IsError { get; }
}