using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wipedir.Executor;
using Wipedir.Update;

namespace Wipedir.CommandLine;
internal class CommandLineParser
{
	#region  - needs -
	private readonly string[] _Arguments;
	private static CommandLineArguments? _ParsedArguments;
    private readonly RootCommand? _RootCommand;
    private readonly GitReleaseManager _GitReleaseManager;
	#endregion

	#region - ctor -
    /// <summary>
    /// Creates an instance of the CommandLineParser.
    /// While instantiation the root command wipedir is created and assigned all the options.
    /// Also the handler to map the settings is assigned.
    /// </summary>
    /// <param name="arguments">The command line arguments.</param>
	internal CommandLineParser(string[] arguments, GitReleaseManager manager) 
	{
		_Arguments = arguments;
		_ParsedArguments = new();
        _RootCommand = __SetupRootCommand();
        _RootCommand.AddCommand(__SetupInstallCommand());
        _GitReleaseManager = manager;
    }
    #endregion

    #region - properties -

    #region [Arguments]
    /// <summary>
    /// Getting the CommandLineArguments object.
    /// </summary>
    public CommandLineArguments Arguments => _ParsedArguments!;
    #endregion

    #endregion

    #region - methods -

    #region [__SetupRootCommand]
    private RootCommand __SetupRootCommand()
    {
        var startingDirectoryOption = new Option<string>(name: "--start", description: "The starting directory") { IsRequired = true };
        startingDirectoryOption.AddAlias("-s");
        var directoriesOption = new Option<string[]>(name: "--dir", description: "Directory to delete. For multiple directories provide the '-d' argument up to 10 times.") { IsRequired = true, Arity = new ArgumentArity(1, 10) };
        directoriesOption.AddAlias("-d");
        var forceOption = new Option<bool>(name: "--force", description: "Force deletion (currently not implemented).", getDefaultValue: () => false);
        forceOption.AddAlias("-f");
        var recursiveOption = new Option<bool>(name: "--recursive", description: "Recursive search.", getDefaultValue: () => false);
        recursiveOption.AddAlias("-r");
        var acknowledgeDeletionOption = new Option<bool>(name: "--yes", description: "Accepting the direct deletion of the found directories without additional button press.", getDefaultValue: () => false);
        acknowledgeDeletionOption.AddAlias("-y");
        var skipFoundFolderPrintingOption = new Option<bool>(name: "--skipFolderPrint", description: "Skips the printing of the found folders before deletion.");
        skipFoundFolderPrintingOption.AddAlias("-sp");
        var errorOutputOption = new Option<string>(name: "--error", description: "Enables the error output into a provided file.", getDefaultValue: () => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "wipedir", $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}-error.txt"));
        errorOutputOption.AddAlias("-e");
        var skipVersionCheckOption = new Option<bool>(name: "--skipVersionCheck", description: "Skips the version check at the beginning.", getDefaultValue: () => false);
        skipVersionCheckOption.AddAlias("-sv");

        var rootCommand = new RootCommand("Wipedir");
        rootCommand.AddOption(startingDirectoryOption);
        rootCommand.AddOption(directoriesOption);
        rootCommand.AddOption(forceOption);
        rootCommand.AddOption(recursiveOption);
        rootCommand.AddOption(acknowledgeDeletionOption);
        rootCommand.AddOption(skipFoundFolderPrintingOption);
        rootCommand.AddOption(errorOutputOption);
        rootCommand.AddOption(skipVersionCheckOption);

        rootCommand.SetHandler((startDir, dirs, force, recursive, acknowledge, skipFoundFolderPrinting, errorOutput, skipVersionCheck) =>
        {
            _ParsedArguments.StartDirectory = startDir;
            _ParsedArguments.DirectoriesToDelete = dirs;
            _ParsedArguments.ForceDeletion = force;
            _ParsedArguments.SearchRecursive = recursive;
            _ParsedArguments.AcknowledgeDeletion = acknowledge;
            _ParsedArguments.SkipFoundFolderPrinting = skipFoundFolderPrinting;
            _ParsedArguments.ErrorOutputFile = errorOutput;
            _ParsedArguments.SkipVersionCheck = skipVersionCheck;
        }, startingDirectoryOption, directoriesOption, forceOption, recursiveOption, acknowledgeDeletionOption, skipFoundFolderPrintingOption, errorOutputOption, skipVersionCheckOption);

        return rootCommand;
    }
    #endregion

    #region [__SetupInstallCommand]
    private Command __SetupInstallCommand()
    {
        var command = new Command("install", "Installs the wipedir as a system wide command.");
        var installDirOption = new Option<string>(name: "--dir", description: "The installation directory." , getDefaultValue: () => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "wipedir"));
        installDirOption.AddAlias("-d");
        var downloadNewestOption = new Option<bool>(name: "--download", description: "Downloads the newest version from GitHub.", getDefaultValue: () => false);
        downloadNewestOption.AddAlias("-dl");

        command.AddOption(installDirOption);
        command.AddOption(downloadNewestOption);

        command.SetHandler((installDir, downloadNewest) =>
        {
            WipedirInstallationExecutor.Install(installDir, downloadNewest, _GitReleaseManager);
        }, installDirOption, downloadNewestOption);

        return command;
    }
    #endregion

    #region [Parse]
    /// <summary>
    /// Parsing the command line arguments.
    /// </summary>
    public async Task Parse()
    {
        await _RootCommand!.InvokeAsync(_Arguments).ConfigureAwait(true);
    }
    #endregion 

    #endregion





}
