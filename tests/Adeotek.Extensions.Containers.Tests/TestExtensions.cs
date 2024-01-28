using System.Text;

namespace Adeotek.Extensions.Containers.Tests;

internal static class TestExtensions
{
    internal static StringBuilder AppendIfNotNullOrEmpty(this StringBuilder sb, string input, string? value) => 
        string.IsNullOrEmpty(value) ? sb : sb.Append(input);
    
    internal static StringBuilder AppendIfNotNullOrEmpty(this StringBuilder sb, string? input) => 
        string.IsNullOrEmpty(input) ? sb : sb.Append(input);
    
    internal static StringBuilder AppendIfNotNullOrEmpty(this StringBuilder sb, string? input, bool check) => 
        check ? sb.Append(input) : sb;
    
    internal static StringBuilder AppendForEach<T>(this StringBuilder sb, IEnumerable<T>? input, Func<T, string?> pattern, bool skipEmptyItems = true)
    {
        if (input is null)
        {
            return sb;
        }

        foreach (T item in input)
        {
            if (skipEmptyItems 
                && (item is null 
                    || (item is string str && string.IsNullOrEmpty(str))))
            {
                continue;
            }
            
            var value = pattern.Invoke(item);
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }
            
            sb.Append(value);
        }
        
        return sb;
    }
}