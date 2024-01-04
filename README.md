# wipedir
![Wipedir Logo](https://github.com/Secodity/wipedir/blob/Logo/Wipedir/Wipedir/Icon/WipeDir.jpeg)

A C# command line tool to remove all folders that follow a given list of names from a starting directory. 

## Getting started

Example execution: `.\wipedir.exe -s C:\ -d .vs -d bin -d obj -r`

| Parameter | Alias | Explanation |
|:---:|:---:|---|
|`--start`|`-s`| The starting directory.|
|`--dir`|`-d`|Directory to delete. For multiple directories provide the `-d` argument up to 10 times.|
|`--recursive`|`-r`|Recursive search.|
|`--yes`|`-y`|Accepting the direct deletion of the found directories without additional button press.|
|`--skipFolderPrint`|`-sp`|Skips the printing of the found folders before deletion.|
|`--error`|`-e`|Enables the error output into a provided file.|

## How to build from source

### Prerequisities
- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Building

- `git clone https://github.com/Secodity/wipedir.git`
- `cd wipedir\Wipedir`
- `dotnet build`

## Acknowledgements

This software uses the following nuget packages: 
- [System.CommandLine](https://www.nuget.org/packages/System.CommandLine)
- [Goblinfactory.Konsole](https://github.com/goblinfactory/konsole/)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
