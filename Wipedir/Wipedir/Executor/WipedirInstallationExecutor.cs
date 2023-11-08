using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wipedir.Update;

namespace Wipedir.Executor;
internal static class WipedirInstallationExecutor
{
    internal static void Install(string directory, bool downloadNewestVersion, GitReleaseManager manager)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new PlatformNotSupportedException().ToString());
            Environment.Exit(1);
        }

        __EnsureDirectoryExists(directory);

        if (downloadNewestVersion)
        {
            __DownloadNewestVersion(directory, manager);
            __UnzipDownload(directory);
        }
        else
            __CopyExecutableFiles(directory);

        __AddDirectoryToPath(directory);
        Environment.Exit(0);
    }

    internal static void Uninstall()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new PlatformNotSupportedException().ToString());
            Environment.Exit(1);
        }

        var currentPathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;
        var pathes = currentPathVariable.Split(";").ToList();
        if (pathes.Count <= 0)
            Environment.Exit(-1);

        var wipeDirPathes = pathes.Where(p => p.Contains("wipedir")).ToList();

        foreach(var wipeDirPath in wipeDirPathes)
        {
            try
            {
                Directory.Delete(wipeDirPath, true);
            } catch { }
            pathes.RemoveAll(p => p.Equals(wipeDirPath));
        }

        Environment.SetEnvironmentVariable("PATH", string.Join(";", pathes), EnvironmentVariableTarget.Machine);
        Environment.Exit(0);
    }

    #region [__AddDirectoryToPath]
    private static void __AddDirectoryToPath(string directory)
    {
        var currentPathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

        var envVar = "PATH";
//#if DEBUG
//        envVar += "_DEV";
//#endif

        if(!currentPathVariable.Contains("wipedir"))
            Environment.SetEnvironmentVariable(envVar, $"{currentPathVariable};{directory}", EnvironmentVariableTarget.Machine);
    } 
    #endregion

    #region [__CopyExecutableFiles]
    private static void __CopyExecutableFiles(string directory)
    {
        var currentPath = Environment.CurrentDirectory;
        File.Copy(Path.Combine(currentPath, "Wipedir.exe"), Path.Combine(directory, "Wipedir.exe"), true);
        File.Copy(Path.Combine(currentPath, "Wipedir.pdb"), Path.Combine(directory, "Wipedir.pdb"), true);
    } 
    #endregion

    #region [__UnzipDownload]
    private static void __UnzipDownload(string directory)
    {
        ZipFile.ExtractToDirectory(Path.Combine(directory, "download.zip"), directory, true);
        File.Copy(Path.Combine(directory, "win-x64", "Wipedir.exe"), Path.Combine(directory, "Wipedir.exe"), true);
        File.Copy(Path.Combine(directory, "win-x64", "Wipedir.pdb"), Path.Combine(directory, "Wipedir.pdb"), true);
    } 
    #endregion

    #region [__DownloadNewestVersion]
    private static void __DownloadNewestVersion(string directory, GitReleaseManager manager)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Anything");
        
        using var response = client.GetAsync(manager.GetDownloadUriForNewestVersion(false, Environment.Is64BitOperatingSystem)).GetAwaiter().GetResult();

        using var streamToReadFrom = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

        using var localStream = new FileStream(path: Path.Combine(directory, "download.zip"), mode: FileMode.Create);

        streamToReadFrom.CopyTo(localStream);
    } 
    #endregion

    #region [__EnsureDirectoryExists]
    private static void __EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory)) 
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch(Exception _)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Couldn't create directory: '{directory}'.");
                Console.WriteLine(_.ToString());
                Console.Write("Press any key to continue...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
    #endregion
}
