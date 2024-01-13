using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wipedir.Update;

namespace Wipedir.Executor;
internal static class WipedirUpdateExecutor
{
    private static readonly string m_INSTALL_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "wipedir");
    private static readonly string m_DOWNLOAD_FOLDER = Path.Combine(m_INSTALL_FOLDER, "download");
    private static readonly string m_DOWNLOAD_FILE = Path.Combine(m_DOWNLOAD_FILE, "download.zip");

    internal static void Install(GitReleaseManager gitReleaseManager, Version version, bool includePreRelease)
    {
        var result = gitReleaseManager.CheckForAvailableUpdate(version, includePreRelease);

        if(!result.UpdateAvailable)
        {
            Console.WriteLine("No update available.");
            _ = Console.ReadKey();
            Environment.Exit(0);
        }

        __Download(gitReleaseManager);
        __UnzipAndCopyDownload(gitReleaseManager);
        __UpdatePathVariable(gitReleaseManager, true);
        RemoveOtherWipedirFolders(gitReleaseManager, false);
    }

    internal static void Uninstall(GitReleaseManager gitReleaseManager)
    {
        __UpdatePathVariable(null, false);
        RemoveOtherWipedirFolders(gitReleaseManager, true);
    }


    internal static void RemoveOtherWipedirFolders(GitReleaseManager gitReleaseManager, bool uninstall)
    {
        var dirs = Directory.GetDirectories(m_INSTALL_FOLDER);
        string currentVersion = string.Empty;
        if(!uninstall)
            currentVersion = $"{gitReleaseManager.NewestVersion.Major}.{gitReleaseManager.NewestVersion.Minor}.{gitReleaseManager.NewestVersion.MinorRevision}";

        foreach (var dir in dirs)
        {
            if (!uninstall && (dir.Equals(currentVersion) || dir.Equals("download")))
                continue;
            Directory.Delete(dir, true);
        }

        File.Delete(m_DOWNLOAD_FILE);
    }

    private static void __UpdatePathVariable(GitReleaseManager gitReleaseManager, bool addNewVersion)
    {
        var currenPathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;
        var pathes = currenPathVariable.Split(";").ToList();

        if (!pathes.Any())
            Environment.Exit(1337);

        var wipeDirPathes = pathes.Where(p => p.Contains("wipedir")).ToList();

        foreach(var wipeDirPath in wipeDirPathes)
        {
            try
            {
                Directory.Delete(wipeDirPath, true);
            }
            catch { /*Not necessary */ }
            _ = pathes.RemoveAll(p => p.Equals(wipeDirPath));
        }

        if (addNewVersion)
            pathes.Add(__CreateVersionizedInstallPath(gitReleaseManager));

        Environment.SetEnvironmentVariable("PATH", string.Join(";", pathes), EnvironmentVariableTarget.Machine);
    }

    private static void __UnzipAndCopyDownload(GitReleaseManager gitReleaseManager)
    {
        var versionizedInstallPath = __CreateVersionizedInstallPath(gitReleaseManager);

        __EnsureDirectoryExists(versionizedInstallPath);
        
        if(!File.Exists(m_DOWNLOAD_FILE))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The file '{m_DOWNLOAD_FILE}' doesn't exist.");
            Console.ResetColor();
            _ = Console.ReadKey();
            Environment.Exit(1);
        }

        ZipFile.ExtractToDirectory(m_DOWNLOAD_FILE, versionizedInstallPath, true);
    }

    private static string __CreateVersionizedInstallPath(GitReleaseManager gitReleaseManager) => Path.Combine(m_INSTALL_FOLDER, $"{gitReleaseManager.NewestVersion.Major}.{gitReleaseManager.NewestVersion.Minor}.{gitReleaseManager.NewestVersion.MinorRevision}");

    private static void __Download(GitReleaseManager gitReleaseManager)
    {
        __EnsureDirectoryExists(m_INSTALL_FOLDER);
        __EnsureDirectoryExists(m_DOWNLOAD_FOLDER);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Anything");

        using var response = client.GetAsync(gitReleaseManager.GetDownloadUriForNewestVersion(false, Environment.Is64BitOperatingSystem)).GetAwaiter().GetResult();

        using var streamToReadFrom = response.Content.ReadAsStream();

        using var localStream = new FileStream(path: m_DOWNLOAD_FILE, mode: FileMode.Create);

        streamToReadFrom.CopyTo(localStream);
    }

    private static void __EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            try
            {
                _ = Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Couldn't create directory: '{directory}'.");
                Console.WriteLine(ex.ToString());
                Console.Write("Press any key to continue...");
                _ = Console.ReadKey();
                Console.ResetColor();
                Environment.Exit(1);
            }
        }
    }
}

