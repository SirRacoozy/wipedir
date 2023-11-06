using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Wipedir.CommandLine;
using Wipedir.Update;

namespace Wipedir;

public static class Program
{
    #region [Main]
    public static void Main(string[] args) => MainAsync(args);
    private static async void MainAsync(string[] args)
    {
        Console.Title = "Wipedir";

        CommandLineParser parser = new(args);
        await parser.Parse();
        if(!parser.Arguments.SkipVersionCheck)
            __CheckForUpdate();

        WipedirExecutor executor = new(parser.Arguments);
        executor.Run();
    }
    #endregion

    private static void __CheckForUpdate()
    {
        var url = "https://api.github.com/repos/Secodity/wipedir/releases";
        using (var client = new HttpClient())
        {
            //Need it here, see https://stackoverflow.com/questions/47576074/get-releases-github-api-v3
            client.DefaultRequestHeaders.Add("User-Agent", "Anything");
            var response = client.GetStringAsync(url).GetAwaiter().GetResult();

            using (var stream = new MemoryStream(Encoding.Default.GetBytes(response)))
            {
                var serializer = new DataContractJsonSerializer(typeof(List<Release>));
                var releases = serializer.ReadObject(stream) as List<Release>;
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                bool updateAvailable = false;

                if (releases.Any(r => r.Version.Major > currentVersion.Major))
                    updateAvailable = true;
                else
                {
                    var releasesOnSameMajor = releases.Where(r => r.Version.Major == currentVersion.Major).ToList();
                    if (!releasesOnSameMajor.Any())
                        return;
                    var greatestMinorVersion = releasesOnSameMajor.Max(r => r.Version.Minor);
                    if (greatestMinorVersion > currentVersion.Minor)
                        updateAvailable = true;
                    else
                    {
                        var releasesOnSameMinor = releases.Where(r => r.Version.Minor == currentVersion.Minor).ToList();
                        if (!releasesOnSameMinor.Any())
                            return;
                        var greatestBuildVersion = releasesOnSameMinor.Max(r => r.Version.Build);
                        if (greatestBuildVersion > currentVersion.Build)
                            updateAvailable = true;
                        else
                        {
                            var releasesOnSameBuild = releases.Where(r => r.Version.Build == currentVersion.Build).ToList();
                            if (!releasesOnSameBuild.Any())
                                return;
                            var greatestMinorRevisionVersion = releasesOnSameBuild.Max(r => r.Version.MinorRevision);
                            if (greatestMinorRevisionVersion > currentVersion.MinorRevision)
                                updateAvailable = true;
                        }
                    }
                }
                if (updateAvailable)
                {
                    Console.Title = "wipedir - Update available";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("A new update is available. Visit https://github.com/repos/Secodity/wipedir/releases to download the new version.\n" +
                        "Press any key to continue...");
                    Console.ReadKey();
                    Console.ResetColor();

                    
                }
            }
        }
    }

    


}
