# Opc2Aml
This C# console application targets .NET Core Runtime 3.1 can be built using Visual Studio (Windows) or Visual Studio Code (Window,Linux,MAC)



## Command Line Help

```
Converts one or more OPC UA Nodeset files in the current working directory (CWD) into their equivalent
AutomationML Libraries.

Opc2Aml.exe [NodesetFile] [AmlBaseFilename]

NodesetFile         Name of nodeset file in the CWD to be processed
AmlBaseFilename     File name of the AutomationML file to be written (without the .amlx extension).
                    The default name is the NodesetFile if this argument is missing.

With no optional arguments, all nodeset files in CWD are processed.
All dependent nodeset files need to be present in the CWD, even if they are not processed.
```



## Binary Releases

The binary releases are provided as a ZIP file. To install the binaries do the following:
1. Install the [.NET Core Runtime 3.1](https://dotnet.microsoft.com/download/dotnet/3.1) for your platform.
2. Extract the files from the ZIP into a directory of your choice.
3. On Windows run "Opc2Aml.exe" from the command line.  On Linux run "dotnet Opc2Aml.dll" from the terminal.
