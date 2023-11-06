using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wipedir.CommandLine;
public class CommandLineArguments
{
    #region - properties -

    #region [StartingDirectory]
    /// <summary>
    /// The starting directory.
    /// </summary>
    public string StartDirectory { get; set; } = string.Empty;
    #endregion

    #region [DirectoriesToDelete]
    /// <summary>
    /// List of directories to search for and to delete.
    /// </summary>
    public string[] DirectoriesToDelete { get; set; } = Array.Empty<string>();
    #endregion

    #region [ErrorOutputFile]
    /// <summary>
    /// The path where the error output should be written to.
    /// </summary>
    public string ErrorOutputFile { get; set; } = string.Empty;
    #endregion

    #region [SearchRecursive]
    /// <summary>
    /// Enables the search to be recursive.
    /// </summary>
    public bool SearchRecursive { get; set; } = false;
    #endregion

    #region [ForceDeletion]
    /// <summary>
    /// Enables the force deletion of folders.
    /// </summary>
    public bool ForceDeletion { get; set; } = false;
    #endregion

    #region [AcknowledgeDeletion]
    /// <summary>
    /// Skips the printing of the "Press any key to continue..." before deletion.
    /// </summary>
    public bool AcknowledgeDeletion { get; set; } = false;
    #endregion

    #region [SkipFoundFolderPrinting]
    /// <summary>
    /// Skips the printing of the found folders.
    /// </summary>
    public bool SkipFoundFolderPrinting { get; set; } = false;
    #endregion

    #region [SkipVersionCheck]
    /// <summary>
    /// Skips the version check.
    /// </summary>
    public bool SkipVersionCheck { get; set; } = false;
    #endregion

    #endregion

    #region - methods -

    #region [ToString]
    public override string ToString()
    {
        return $"StartDirectory: {StartDirectory}\n" +
            $"DirectoriesToDelete: {string.Join(",", DirectoriesToDelete)}\n" +
            $"SearchRecursive: {SearchRecursive}\n" +
            $"ForceDeletion: {ForceDeletion}\n" +
            $"AcknowledgeDeletion: {AcknowledgeDeletion}\n" +
            $"SkipFoundFolderPrinting: {SkipFoundFolderPrinting}\n" +
            $"ErrorOutputDirectory: {ErrorOutputFile}\n" +
            $"SkipVersionCheck: {SkipVersionCheck}";
    }
    #endregion 

    #endregion
}
