using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Processes;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class Utf8BomBaseCommand<TSettings> 
    : CommandBase<TSettings> where TSettings : Utf8BomSettings
{
    protected int ProcessDirectory(string targetPath, Func<string, int> processFile, 
        string fileExtensions = "", string ignoreDirs = "")
    {
        if (!Directory.Exists(targetPath))
        {
            throw new ShellCommandException(1, "The target directory does not exist");
        }
        
        // DirectoryInfo dirInfo = new(targetPath);
        // var files = from f in dirInfo.EnumerateFiles()
        //     where f.CreationTimeUtc < StartOf2009
        //     select f;
        //
        // var files = from f in Directory.EnumerateFileSystemEntries(targetPath)
        //     where f.CreationTimeUtc < StartOf2009
        //     select f;
        
        return 0;
    }
    
    
}