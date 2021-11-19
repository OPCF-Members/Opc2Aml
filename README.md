# Opc2Aml

Command line utility to convert OPC UA Nodeset files to AutomationML Libraries for UAFX offline engineering.

## Command line help

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

## Building from source

This C# console application targets .NET Core Runtime 3.1 can be built using Visual Studio (Windows) or Visual Studio Code (Window,Linux,MAC).

## Binary releases

The binary releases are provided as a ZIP file. To install the binaries do the following:

1. Install the [.NET Core Runtime 3.1](https://dotnet.microsoft.com/download/dotnet/3.1) for your platform.
2. Extract the files from the ZIP into a directory of your choice.
3. Run the app:
   a. Windows command line> ` Opc2Aml.exe`
   b. Linux terminal> `dotnet Opc2Aml.dll`

## Licenses

The source code in this repository is MIT License but the code depends on two NuGet packages with the following licenses:

- AML.Engine -  [Nuget MIT License](https://licenses.nuget.org/MIT).
- OPCFoundation.NetStandard.Opc.UA - [Custom License](https://www.nuget.org/packages/OPCFoundation.NetStandard.Opc.Ua/1.4.367.42/license)
