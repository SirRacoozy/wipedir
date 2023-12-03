using System.Reflection;
using Wipedir.CommandLine;
using Wipedir.Executor;
using Wipedir.Update;

namespace Wipedir;

public static class Program
{
    #region [Main]
    public static void Main(string[] args) => __MainAsync(args);
    private static async void __MainAsync(string[] args)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var ReleaseManager = new GitReleaseManager(currentVersion, "https://api.github.com/repos/Secodity/wipedir/releases");

        Console.Title = "Wipedir";

        CommandLineParser parser = new(args, ReleaseManager);
        await parser.Parse();
        if (!parser.Arguments.SkipVersionCheck)
            __CheckForUpdate(ReleaseManager);

        WipedirExecutor executor = new(parser.Arguments);
        executor.Run();
    }
    #endregion

    private static void __CheckForUpdate(GitReleaseManager releaseManager)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Title = "wipedir - Checking for an updated Version";
        Console.WriteLine("wipedir - Checking for an updated Version");

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var result = releaseManager.CheckForAvailableUpdate(currentVersion, false);

        if (result.UpdateAvailable)
        {
            Console.Title = "wipedir - Update available";
            Console.WriteLine($"A new update is available. Version {result.NewestVersion}\n" +
                        "Visit https://github.com/repos/Secodity/wipedir/releases to download the new version.\n" +
                        "Press any key to continue...");
            _ = Console.ReadKey();
        }
        else
            Console.Title = "Wipedir";

        Console.ResetColor();
    }
}
