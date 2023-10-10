using System.CommandLine;

namespace Wipedir;

public static class Program
{
    public static void Main(string[] args) => __MainAsync(args);
    private async static void __MainAsync(string[] args)
    {
        Console.Title = "Wipedir";

        var startingDirectory = new Option<string>(name: "--start", description: "The starting directory");
        startingDirectory.AddAlias("-s");
        var directory = new Option<string>(name: "--dirs", description: "Directories to delete");
        directory.AddAlias("-d");
        var force = new Option<bool>(name: "--force", description: "Force deletion", getDefaultValue: () => false);
        force.AddAlias("-f");
        var recursive = new Option<bool>(name: "--recursive", description: "Recursive search", getDefaultValue: () => false);
        recursive.AddAlias("-r");


        var rootCommand = new RootCommand("Wipedir");
        rootCommand.AddOption(startingDirectory);
        rootCommand.AddOption(directory);
        rootCommand.AddOption(force);
        rootCommand.AddOption(recursive);

        rootCommand.SetHandler((startDirValue, dirValue, forceValue, recursiveValue) =>
        {
            __Execute(startDirValue, dirValue, forceValue, recursiveValue);
        }, startingDirectory, directory, force, recursive);

        await rootCommand.InvokeAsync(args);
    }

    private static void __Execute(string startDir, string directory, bool force, bool recursive)
    {
        Console.WriteLine($"StartDir: {startDir}");
        Console.WriteLine($"Directories: {String.Join(",", directory)}");
        Console.WriteLine($"Force: {force}");
        Console.WriteLine($"Recursive: {recursive}");
    }
}
