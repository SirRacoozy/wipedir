using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wipedir.CommandLine;
public class CommandLineArguments
{
    public string StartDirectory { get; set; } = string.Empty;
    public string[] DirectoriesToDelete { get; set; } = Array.Empty<string>();
    public string ErrorOutputFile { get; set; } = string.Empty;
    public bool SearchRecursive { get; set; } = false;
    public bool ForceDeletion { get; set; } = false;
    public bool AcknowledgeDeletion { get; set; } = false;
    public bool SkipFoundFolderPrinting { get; set; } = false;

    public override string ToString()
    {
        return $"StartDirectory: {StartDirectory}\n" +
            $"DirectoriesToDelete: {string.Join(",", DirectoriesToDelete)}\n" +
            $"SearchRecursive: {SearchRecursive}\n" +
            $"ForceDeletion: {ForceDeletion}\n" +
            $"AcknowledgeDeletion: {AcknowledgeDeletion}\n" +
            $"SkipFoundFolderPrinting: {SkipFoundFolderPrinting}\n" +
            $"ErrorOutputDirectory: {ErrorOutputFile}";
    }
}
