using System.CommandLine;
using CodeBureau.DeleteOld;

// Simple CLI option parsing
var options = new DeleteOldOptions();
bool showHelp = false;

for (int i = 0; i < args.Length; i++)
{
    var arg = args[i].ToLower();
    
    if (arg.StartsWith("-p") || arg.StartsWith("--path"))
    {
        options.Path = GetOptionValue(args, i++, options.Path);
    }
    else if (arg.StartsWith("-f") || arg.StartsWith("--filter"))
    {
        options.Filter = GetOptionValue(args, i++, options.Filter);
    }
    else if (arg.StartsWith("-a") || arg.StartsWith("--age"))
    {
        var ageStr = GetOptionValue(args, i++, "0");
        if (int.TryParse(ageStr, out var age))
            options.Age = age;
    }
    else if (arg.StartsWith("-t") || arg.StartsWith("--timeframe"))
    {
        options.TimeFrame = GetOptionValue(args, i++, options.TimeFrame);
    }
    else if (arg == "-s" || arg == "--recurse")
    {
        options.RecurseSubfolders = true;
    }
    else if (arg == "-r" || arg == "--remove-empty")
    {
        options.RemoveEmptyFolders = true;
    }
    else if (arg == "-q" || arg == "--quiet")
    {
        options.QuietMode = true;
    }
    else if (arg == "-n" || arg == "--simulate")
    {
        options.SimulateOnly = true;
    }
    else if (arg == "-h" || arg == "--help")
    {
        showHelp = true;
        break;
    }
}

if (showHelp)
{
    Console.WriteLine(DeleteOldService.GetUsage());
    Environment.Exit(0);
}

var service = new DeleteOldService();
var exitCode = service.Execute(options);
Environment.Exit(exitCode);

static string GetOptionValue(string[] args, int index, string defaultValue)
{
    if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
        return args[index + 1];
    return defaultValue;
}
