using Konsole;
using System.Collections.Concurrent;
using System.CommandLine;

namespace Wipedir;

public static class Program
{

    #region - needs -
    private static ConcurrentBag<string> _DeleteErrorMessages = new ConcurrentBag<string>();
    private static long _CurrentFileProgress = 0;
    private static ConcurrentBag<string> _Paths = new ConcurrentBag<string>();
    #endregion

    #region [Main]
    public static void Main(string[] args) => __MainAsync(args);
    #endregion

    #region [__MainAsync]
    private async static void __MainAsync(string[] args)
    {
        Console.Title = "Wipedir";

        var startingDirectoryOption = new Option<string>(name: "--start", description: "The starting directory") { IsRequired = true };
        startingDirectoryOption.AddAlias("-s");
        var directoriesOption = new Option<string[]>(name: "--dir", description: "Directory to delete. For multiple directories provide the '-d' argument up to 10 times.") { IsRequired = true, Arity = new ArgumentArity(1, 10) };
        directoriesOption.AddAlias("-d");
        var forceOption = new Option<bool>(name: "--force", description: "Force deletion (currently not implemented)", getDefaultValue: () => false);
        forceOption.AddAlias("-f");
        var recursiveOption = new Option<bool>(name: "--recursive", description: "Recursive search", getDefaultValue: () => false);
        recursiveOption.AddAlias("-r");
        var acknowledgeDeletionOption = new Option<bool>(name: "--yes", description: "Accepting the direct deletion of the found directories without additional button press.", getDefaultValue: () => false);
        acknowledgeDeletionOption.AddAlias("-y");


        var rootCommand = new RootCommand("Wipedir");
        rootCommand.AddOption(startingDirectoryOption);
        rootCommand.AddOption(directoriesOption);
        rootCommand.AddOption(forceOption);
        rootCommand.AddOption(recursiveOption);
        rootCommand.AddOption(acknowledgeDeletionOption);

        CommandLineArguments Arguments = null;

        rootCommand.SetHandler((startDirValue, dirValue, forceValue, recursiveValue) =>
        {
            Arguments = new CommandLineArguments()
            {
                StartDirectory = startDirValue,
                DirectoriesToDelete = dirValue,
                ForceDeletion = forceValue,
                SearchRecursive = recursiveValue,

            };
        }, startingDirectoryOption, directoriesOption, forceOption, recursiveOption);

        await rootCommand.InvokeAsync(args);

        if (Arguments == null)
        {
            return;
        }
        __Execute(Arguments);
    }
    #endregion

    #region [__FindFoldersFromStart]
    private static void __FindFoldersFromStart(string startDirectory, string[] directoriesToDelete, ProgressBar progressBar)
    {
        try
        {
            var directories = Directory.GetDirectories(startDirectory).ToList();
            var matches = directories.Where(foundDirectory => directoriesToDelete.Any(directoryToDelete => foundDirectory.EndsWith(directoryToDelete))).ToList();
            matches.ForEach(x => _Paths.Add(x));
            directories.RemoveAll(x => matches.Any(y => x.Equals(y)));

            Parallel.ForEach(directories, dir => __FindFoldersFromStart(dir, directoriesToDelete, progressBar));
        }
        catch (Exception ex) { }
    }
    #endregion

    #region [__Execute]
    private static void __Execute(CommandLineArguments arguments)
    {
#if DEBUG
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(arguments);
        Console.ResetColor();
#endif
        __ValidateStartDirectory(arguments.StartDirectory);

        var progressBar = new ProgressBar(100);
        progressBar.Refresh(0);

        __FindFoldersFromStart(arguments.StartDirectory, arguments.DirectoriesToDelete, progressBar);

        progressBar.Refresh(100);

        Console.ReadKey();

        Console.WriteLine(string.Join("\n", _Paths));

        Console.WriteLine($"{_Paths.Count} folders found.");

        if (!arguments.AcknowledgeDeletion)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }

        __RemoveFolders(_Paths.ToArray(), arguments.ForceDeletion);
    }
    #endregion

    #region [__RemoveFolders]
    private static void __RemoveFolders(string[] directories, bool force)
    {
        Console.Clear();
        var progressBar = new ProgressBar(PbStyle.DoubleLine, directories.Length);
        progressBar.Refresh(0);
        Parallel.ForEach(directories, dir =>
        {
            __RemoveFolder(dir, progressBar);
        });
    }
    #endregion

    #region [__RemoveFolder]
    private static void __RemoveFolder(string directory, ProgressBar progressBar)
    {
        try
        {
            Directory.Delete(directory, true);

        }
        catch (Exception ex)
        {
            _DeleteErrorMessages.Add(ex.Message);
        }
        finally
        {
            Interlocked.Increment(ref _CurrentFileProgress);
            progressBar.Refresh((int)Interlocked.Read(ref _CurrentFileProgress));
        }
    }
    #endregion

    #region [__ValidateStartDirectory]
    private static void __ValidateStartDirectory(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The argument '-s' can't be null or empty or only containing whitespaces.");
            Environment.Exit(1);
        }

        if (!Directory.Exists(value))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The argument -s with the value '{value}' is not a directory.");
            Environment.Exit(1);
        }
    }
    #endregion

}
