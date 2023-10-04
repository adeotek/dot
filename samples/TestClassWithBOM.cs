namespace NoNamespace;

public static class TestClassWithBOM
{
    /// <summary>
    /// Some test function
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Test(string input) =>
        string.IsNullOrEmpty(input) ? "[null]" : input.Upper();
}
