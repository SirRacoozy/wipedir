using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Wipedir.CommandLine;

namespace Wipedir;

public static class Program
{
  #region [Main]
  public static void Main(string[] args) => MainAsync(args);
  private static async void MainAsync(string[] args)
  {
    __CheckForUpdate();
    CommandLineParser parser = new(args);
    await parser.Parse();

    WipedirExecutor executor = new(parser.Arguments);
    executor.Run();
  }
  #endregion

  private static async void __CheckForUpdate()
  {
    var url = "https://api.github.com/repos/Secodity/wipedir/releases";
    using (var client = new WebClient())
    {
      //Need it here, see https://stackoverflow.com/questions/47576074/get-releases-github-api-v3
      client.Headers.Add("User-Agent", "Anything");
      var response = client.DownloadString(url);

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
          Console.Title = "wipedir - Update available";
      }
    }
  }

  [DataContract]
  public class Release
  {
    private Version version;
    [DataMember]
    public string tag_name { get; set; }

    [DataMember]
    public string name { get; set; }

    public Version Version
    {
      get
      {
        var versionString = name.Substring(1);
        if (versionString.StartsWith("."))
          versionString = versionString.Substring(1);
        if (version == null && name.ToString().Length >= 1)
          version = Version.Parse(versionString);
        return version;
      }
    }
  }


}
