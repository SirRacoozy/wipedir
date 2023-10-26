using Konsole;
using System.Collections.Concurrent;
using Wipedir.CommandLine;

namespace Wipedir;
public class WipedirExecutor
{
    #region - needs -
    private readonly CommandLineArguments _Arguments;
    private readonly ConcurrentBag<(string Caller, Exception ex)> _Exceptions;
    #endregion

    #region - ctor -
    public WipedirExecutor(CommandLineArguments args)
    {
        _Arguments = args;
        _Exceptions = new();
    }
    #endregion

    #region [Run]
    public void Run()
    {
#if DEBUG
        __PrintArguments();
#endif

        __ValidateStartDirectory(_Arguments.StartDirectory);

        if(string.IsNullOrEmpty(_Arguments.ErrorOutputFile))
            __ValidateErrorOutputFile(_Arguments.ErrorOutputFile);

        var folders = __FindAllFoldersToDelete(_Arguments);
        if(!_Arguments.SkipFoundFolderPrinting)
            __PrintFoundFolders(folders);
        var removedFilesCount = __RemoveFolders(folders, _Arguments.ForceDeletion);
        Console.WriteLine($"Deleted {removedFilesCount} files.");
        if(!string.IsNullOrEmpty(_Arguments.ErrorOutputFile) && _Exceptions.Count > 0)
            __WriteErrorsIntoFile(_Arguments.ErrorOutputFile);
        __PressKeyToContinue();
    }

    

    #endregion

    #region [__PrintArguments]
    private void __PrintArguments()
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(_Arguments);
        __PressKeyToContinue();
        Console.ResetColor();
    }
    #endregion

    #region [__ValidateStartDirectory]
    private void __ValidateStartDirectory(string path)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The argument '-s' can't be null or empty or only containing whitespaces.");
            Environment.Exit(1);
        }

        if (!Directory.Exists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The argument -s with the path '{path}' is not a directory.");
            Environment.Exit(1);
        }
    }
    #endregion
    #region [__FindAllFoldersToDelete]
    private List<string> __FindAllFoldersToDelete(CommandLineArguments arguments)
    {
        var cancellationToken = new CancellationTokenSource();
        var Folders = new List<string>();

        Parallel.Invoke(
            () => __SpinBusyIndicator(cancellationToken.Token),
            () =>
            {
                Folders.AddRange(__FindFolders(arguments.StartDirectory, arguments.DirectoriesToDelete, arguments.SearchRecursive));
                cancellationToken.Cancel();
            }
        );
        return Folders;
    }
    #endregion

    #region [__FindFolders]
    private List<string> __FindFolders(string startDirectory, string[] directoriesToDelete, bool searchRecursive)
    {
        ConcurrentBag<string> paths = new();
        try
        {
            var directories = Directory.GetDirectories(startDirectory).ToList();
            var matches = directories.Where(foundDirectory => directoriesToDelete.Any(
                                            directoryToDelete => (foundDirectory.Split("\\").LastOrDefault() ?? string.Empty)
                                            .Equals(directoryToDelete))).ToList();

            matches.ForEach(x => paths.Add(x));
            directories.RemoveAll(x => matches.Any(y => x.Equals(y)));

            if (searchRecursive)
                Parallel.ForEach(directories, dir => __FindFolders(dir, directoriesToDelete, searchRecursive).ForEach(item => paths.Add(item)));
        }
        catch(Exception ex) { _Exceptions.Add((nameof(__FindFolders), ex)); }
        return paths.ToList();
    }
    #endregion

    #region [__SpinBusyIndicator]
    private void __SpinBusyIndicator(CancellationToken cancellationToken)
    {
        var spinSequence = new string[] { "|", "/", "-", "\\" };

        for (int i = 0; !cancellationToken.IsCancellationRequested; i++)
        {
            Console.Clear();
            Console.Write($"Searching: \t{spinSequence[i]}");
            if (i == spinSequence.Length - 1)
                i = 0;
            Thread.Sleep(100);
        }
    }
    #endregion

    #region [__PrintFoundFolders]
    private void __PrintFoundFolders(IEnumerable<string> folders)
    {
        Console.WriteLine(string.Join(Environment.NewLine, folders));
        Console.WriteLine($"{folders.Count()} folders found.");

        if(!_Arguments.AcknowledgeDeletion)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Deletion will start right after.");
            __PressKeyToContinue();
            Console.ResetColor();
        }
    }
    #endregion

    #region [__RemoveFolders]
    private long __RemoveFolders(List<string> directories, bool force)
    {
        Console.Clear();

        long currentFileProgress = 0;

        var progressBar = new ProgressBar(PbStyle.DoubleLine, directories.Count);
        progressBar.Refresh(0);

        Parallel.ForEach(directories, dir =>
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                _Exceptions.Add((nameof(__RemoveFolders), ex));
            }
            finally
            {
                Interlocked.Increment(ref currentFileProgress);
                progressBar.Refresh((int)Interlocked.Read(ref currentFileProgress));
            }
        });
        return currentFileProgress;
    }
    #endregion

    #region [__PressKeyToContinue]
    private void __PressKeyToContinue()
    {
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
    #endregion

    #region [__WriteErrorsIntoFile]
    private void __WriteErrorsIntoFile(string filePath)
    {
        File.WriteAllText(filePath, string.Join("\n", _Exceptions.Select(e => e.ToString())));
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{_Exceptions.Count} exceptions occurred. Exceptions where written to '{_Arguments.ErrorOutputFile}'!");
    }
    #endregion

    #region [__ValidateErrorOutputFile]
    private void __ValidateErrorOutputFile(string filePath)
    {
        try
        {
            File.Create(filePath).Dispose();
        } catch(Exception _)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_.ToString());
            Environment.Exit(1);
        }
    }
    #endregion

}
