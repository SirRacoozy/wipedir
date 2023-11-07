using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wipedir.Update;

[DataContract]
internal class Release
{
    private Version version;
    [DataMember]
    public string tag_name { get; set; }

    [DataMember]
    public string name { get; set; }

    [DataMember]
    public bool prerelease { get; set; }

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

