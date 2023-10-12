using System.Text;

using Adeotek.DevOpsTools.CommandsSettings;
using Adeotek.DevOpsTools.Common;
using Adeotek.Extensions.Processes;

using Spectre.Console;

namespace Adeotek.DevOpsTools.Commands;

internal abstract class Utf8BomBaseCommand<TSettings> 
    : CommandBase<TSettings> where TSettings : Utf8BomSettings
{
    protected readonly UTF8Encoding _utf8NoBom = new(false);
    protected readonly UTF8Encoding _utf8WithBom = new(true);
    
    protected uint _totalFiles;
    protected uint _affectedFiles;
    
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
            PrintSeparator();
        }

        ProcessDirectory(
            removeBom ? RemoveBom : AddBom,
            targetPath,
            fileExtensions,
            ignoreDirs,
            dryRun
        );

        if (dryRun)
        {
            PrintMessage("Dry run: No changes were made!", _warningColor, IsVerbose || _affectedFiles > 0);
            PrintMessage($"{_affectedFiles} files to be modified of {_totalFiles} total processed files", 
                _affectedFiles > 0 ? _successColor : _standardColor);
            return 0;
        }
        
        PrintMessage($"{_affectedFiles} files modified of {_totalFiles} total processed files", 
            _affectedFiles > 0 ? _successColor : _standardColor, IsVerbose || _affectedFiles > 0);
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
        var fileEncoding = GetTextFileEncoding(file);
        if (fileEncoding != TextFileEncoding.NoBOM)
        {
            PrintFileResult(0, file, fileEncoding);
            return;
        }

        if (!dryRun)
        {
            var content = File.ReadAllText(file, _utf8NoBom);
            File.WriteAllText(file, content, _utf8WithBom);
            Changes++;
        }
        PrintFileResult(1, file, fileEncoding, dryRun);
    }
    
    protected virtual void RemoveBom(string file, bool dryRun)
    {
        var fileEncoding = GetTextFileEncoding(file);
        if (fileEncoding != TextFileEncoding.UTF8BOM)
        {
            PrintFileResult(0, file, fileEncoding);
            return;
        }
        
        if (!dryRun)
        {
            var content = File.ReadAllText(file, _utf8WithBom);
            File.WriteAllText(file, content, _utf8NoBom);
            Changes++;
        }
        PrintFileResult(-1, file, fileEncoding, dryRun);
    }

    protected virtual TextFileEncoding GetTextFileEncoding(string file)
    {
        if (string.IsNullOrEmpty(file) || !File.Exists(file))
        {
            throw new ShellCommandException(1, $"File not found or invalid: {file}");
        }
        
        using Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
        var bom = new byte[3];
        var count = stream.Read(bom, 0, 3);
        _totalFiles++;
 
        // UTF-8 (Encoding.UTF8) BOM is like 0xEF, 0xBB, 0xBF
        if (count > 2 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) 
        {
            return TextFileEncoding.UTF8BOM;
        }
        
        //UTF-16LE (Encoding.Unicode) BOM is like 0xFF, 0xFE
        if (count > 1 && bom[0] == 0xFF && bom[1] == 0xFE)
        {
            return TextFileEncoding.UTF16LE;
        }
        
        //UTF-16BE (Encoding.BigEndianUnicode) BOM is like 0xFE, 0xFF
        if (count > 1 && bom[0] == 0xFE && bom[1] == 0xFF)
        {
            return TextFileEncoding.UTF16BE;
        }

        return TextFileEncoding.NoBOM;
    }

    protected virtual void PrintFileResult(int action, string file, TextFileEncoding textFileEncoding, bool dryRun = false)
    {
        if (action == 0)
        {
            if (IsVerbose)
            {
                AnsiConsole.Write(new CustomComposer()
                    .Style(_standardColor, "(*)").Space()
                    .Style(_verboseColor, file)
                    .Space().Style(_standardColor, "|>").Space()
                    .Style(_successColor, textFileEncoding.ToString()).LineBreak());
            }
            
            return;
        }
        
        _affectedFiles++;
        var indicator = dryRun ? "(!)" : action == 1 ? "(+)" : "(-)";
        AnsiConsole.Write(new CustomComposer()
            .Style(dryRun ? _standardColor : _successColor, indicator).Space()
            .Style(_verboseColor, file)
            .Space().Style(_standardColor, "|>").Space()
            .Style(_warningColor, textFileEncoding.ToString())
            .Space().Style(_standardColor, "->").Space()
            .Style(_successColor, action == 1 ? TextFileEncoding.UTF8BOM.ToString() : "UTF8 without BOM")
            .LineBreak());
    }
}