using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.Extensions.Processes;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class Utf8BomBaseCommand<TSettings> 
    : CommandBase<TSettings> where TSettings : Utf8BomSettings
{
    protected uint _totalFiles = 0;
    protected uint _affectedFiles = 0;
    
    protected int ProcessTarget(
        bool removeBom,
        string targetPath, 
        string[] fileExtensions, 
        string[] ignoreDirs, 
        bool dryRun)
    {
        if (string.IsNullOrEmpty(targetPath) || !Directory.Exists(targetPath))
        {
            throw new ShellCommandException(1, "The target directory is invalid or does not exist");
        }

        if (IsVerbose)
        {
            PrintMessage("Scanning target path: ", skipLineBreak: true);
            PrintMessage(targetPath, _verboseColor);
            PrintMessage("For files:");
            foreach (var ext in fileExtensions)
            {
                PrintMessage($"|>    *{ext}", _verboseColor);
            }
            PrintMessage("Skipping sub-directories:");
            foreach (var dir in ignoreDirs)
            {
                PrintMessage($"|>    {dir}", _warningColor);
            }
        }

        ProcessDirectory(
            removeBom ? RemoveBom : AddBom,
            targetPath,
            fileExtensions,
            ignoreDirs,
            dryRun
        );
        
        PrintMessage($"{_affectedFiles} files modified of {_totalFiles} total processed files", 
            _totalFiles > 0 ? _successColor : _warningColor, IsVerbose);
        return 0;
    }

    protected virtual void ProcessDirectory(
        Action<string, bool> processFile,
        string path,
        string[] fileExtensions,
        string[] ignoreDirs,
        bool dryRun)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            throw new ShellCommandException(1, $"Directory not found or invalid: {path}");
        }
        
        var subDirectories = Directory.EnumerateDirectories(path);
        if (ignoreDirs.Length > 0)
        {
            subDirectories = subDirectories.Where(d => !ignoreDirs.Contains(Path.GetFileName(d)));
        }
        foreach (var dir in subDirectories)
        {
            ProcessDirectory(processFile, dir, fileExtensions, ignoreDirs, dryRun);
        }
        
        var files = Directory.EnumerateFiles(path);
        if (fileExtensions.Length > 0)
        {
            files = files.Where(f => fileExtensions.Contains(Path.GetExtension(f)));
        }
        foreach (var file in files)
        {
            processFile(file, dryRun);
        }
    }

    protected virtual void AddBom(string file, bool dryRun)
    {
        if (string.IsNullOrEmpty(file) || !File.Exists(file))
        {
            throw new ShellCommandException(1, $"File not found or invalid: {file}");
        }

        _totalFiles++;
        PrintMessage($"+|> {file} | {Path.GetExtension(file)}", _warningColor);
    }
    
    protected virtual void RemoveBom(string file, bool dryRun)
    {
        if (string.IsNullOrEmpty(file) || !File.Exists(file))
        {
            throw new ShellCommandException(1, $"The file [{file}] is invalid or does not exist!");
        }

        _totalFiles++;
        PrintMessage($"-|> {file}", _warningColor);
    }
}