using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wipedir;
internal class CommandLineArguments
{
    internal string StartDirectory { get; set; } = string.Empty;
    internal string[] DirectoriesToDelete { get; set; } = Array.Empty<string>();
    internal bool SearchRecursive { get; set; } = false;
    internal bool ForceDeletion { get; set; } = false;

    public override string ToString()
    {
        return $"StartDirectory: {StartDirectory}\nDirectoriesToDelete: {string.Join(",", DirectoriesToDelete)}\nSearchRecursive: {SearchRecursive}\nForceDeletion: {ForceDeletion}";
    }
}
