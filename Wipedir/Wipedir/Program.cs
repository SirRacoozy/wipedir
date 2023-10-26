using Konsole;
using System.Collections.Concurrent;
using System.CommandLine;
using Wipedir.CommandLine;

namespace Wipedir;

public static class Program
{
    #region [Main]
    public static void Main(string[] args) => MainAsync(args);
    private static async void MainAsync(string[] args)
    {
        CommandLineParser parser = new(args);
        await parser.Parse();

        WipedirExecutor executor = new(parser.Arguments);

        executor.Run();
    }
    #endregion

    

    

}
