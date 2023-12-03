using System.Runtime.Serialization;

namespace Wipedir.Update;

[DataContract]
internal class Release
{
#pragma warning disable IDE1006 // Naming Styles
    private Version version;

    [DataMember]
    public string tag_name { get; set; }

    [DataMember]
    public string name { get; set; }

    [DataMember]
    public bool prerelease { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    public Version Version
    {
        get
        {
            var versionString = name[1..];
            if (versionString.StartsWith('.'))
                versionString = versionString.Substring(1);
            if (version == null && name.ToString().Length >= 1)
                version = Version.Parse(versionString);
            return version;
        }
    }
}

