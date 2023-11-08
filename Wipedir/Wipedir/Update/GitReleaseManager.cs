using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Wipedir.Update;
internal class GitReleaseManager
{
    #region - ctor -
    public GitReleaseManager(Version currentVersion, string Uri)
    {
        RepoUri = new(Uri);
        __GetReleases();
        NewestVersion = CheckForAvailableUpdate(currentVersion, false).NewestVersion;
    }
    #endregion

    #region - properties -

    #region [RepoUri]
    internal Uri RepoUri { get; set; }
    #endregion

    #region [Releases]
    internal List<Release> Releases { get; set; } = new();
    #endregion

    #region [NewestVersion]
    public Version? NewestVersion { get; set; }
    #endregion

    #endregion

    #region - methods -

    #region [CheckForAvailableUpdate]
    public (bool UpdateAvailable, Version? NewestVersion) CheckForAvailableUpdate(Version CurrentVersion, bool CheckForPrelease)
    {
        bool updateAvailable = false;
        if (Releases == null || CurrentVersion == null)
            return (updateAvailable, null);

        var gitReleases = CheckForPrelease ? Releases : Releases.Where(r => !r.prerelease).ToList();

        if (gitReleases.Any(r => r.Version.Major > CurrentVersion.Major))
            updateAvailable = true;
        else
        {
            gitReleases = gitReleases.Where(r => r.Version.Major ==  CurrentVersion.Major).ToList();
            if (!gitReleases.Any())
                return (updateAvailable, null);

            var greatestMinorVersion = gitReleases.Max(r => r.Version.Minor);
            if(greatestMinorVersion > CurrentVersion.Minor)
                updateAvailable = true;
            else
            {
                gitReleases = gitReleases.Where(r => r.Version.Minor == CurrentVersion.Minor).ToList();
                if(!gitReleases.Any())
                    return (updateAvailable, null);

                var greatestBuildVersion = gitReleases.Max(r => r.Version.Build);
                if(greatestBuildVersion > CurrentVersion.Build)
                    updateAvailable = true;
                else
                {
                    gitReleases = gitReleases.Where(r => r.Version.Build == CurrentVersion.Build).ToList();
                    if(!gitReleases.Any())
                        return (updateAvailable, null);

                    var greatestMinorRevisionVersion = gitReleases.Max(r => r.Version.MinorRevision);
                    if(greatestMinorRevisionVersion > CurrentVersion.MinorRevision)
                        updateAvailable = true;
                }
            }
        }
        NewestVersion = Releases.OrderByDescending(r => r.Version.Major)
                                          .ThenByDescending(r => r.Version.Minor)
                                          .ThenByDescending(r => r.Version.Build)
                                          .ThenByDescending(r => r.Version.MinorRevision)
                                          .FirstOrDefault()?.Version;
        return (updateAvailable, NewestVersion);
    }
    #endregion

    #region [GetDownloadUriForNewestVersion]
    public Uri GetDownloadUriForNewestVersion(bool usePortable, bool useX64)
    {
        // https://github.com/Secodity/wipedir/releases/download/v1.1.1/wipedir-portable-v1.1.1.zip
        // https://api.github.com/repos/Secodity/wipedir/releases

        var versionString = $"v{NewestVersion.Major}.{NewestVersion.Minor}.{NewestVersion.Build}";
        var downloadUrl = RepoUri.ToString();
        downloadUrl = downloadUrl.Replace("api.", string.Empty);
        downloadUrl = downloadUrl.Replace("/repos", string.Empty);
        downloadUrl = Path.Combine(downloadUrl, "download", versionString);

        if (usePortable)
            return new Uri(Path.Combine(downloadUrl, $"wipedir-portable-{versionString}.zip"));
        else
            return new Uri(Path.Combine(downloadUrl, $"wipedir-{(useX64 ? "win-x64" : "win-x86")}-{versionString}.zip"));
    }
    #endregion

    #region [__GetReleases]
    private void __GetReleases()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Anything");
        var response = client.GetStringAsync(RepoUri).GetAwaiter().GetResult();

        using var stream = new MemoryStream(Encoding.Default.GetBytes(response));
        var serializer = new DataContractJsonSerializer(typeof(List<Release>));
        Releases = (serializer.ReadObject(stream) as List<Release>)?? new();
    }
    #endregion

    #endregion
}