/* ========================================================================
 * Copyright (c) 2005-2021 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/


// Disabling this define allows the program to bypass the try/catch
// for better debugging error scenarios
#define ENABLE_PROGRAM_EXCEPTION

using System;
using System.IO;
using System.Collections.Generic;
using MarkdownProcessor;
using Microsoft.Extensions.Configuration;
using Opc.Ua;

namespace Opc2Aml
{

    internal class Program
    {
        private static Dictionary<string, string> Models;  // dictionary of model URIs (key) with Nodeset filenames (value) for nodeset files in the CWD
        private Program()
        {
            Initialize();

            Models = new Dictionary<string, string>();
            // load the dictionary with the UA models available in the Current Working Directory (CWD)
            string[] fileEntries = Directory.GetFiles("./", "*.xml");
            foreach (string fileEntry in fileEntries)
            {
                ModelManager manager = new ModelManager();
                string uri;
                Console.WriteLine("Loading nodeset: " + fileEntry + "  ...");
                Utils.LogInfo( "Loading nodeset: " + fileEntry + "  ..." );
                try
                {
                    uri = manager.LoadModel(fileEntry, null, null);
                }
                catch (Exception ex )
                {
                    Console.WriteLine("Unable to load nodeset: " + fileEntry + "  Are you missing a <Uri> element or is the file not a proper nodeset?");
                    Utils.LogError( "Unable to load nodeset: " + fileEntry + " [{0}]", ex.Message );
                    throw;
                }
                if( uri != null)
                    Models.Add(uri, fileEntry.Substring(2));
            }
        }

        private void Initialize()
        {
            _configuration = new Configuration();
            _configuration.Load();
            _configuration.TraceConfiguration.ApplySettings();
        }

        static void ModelImporter_ModelRequired(object sender, ModelRequiredEventArgs e)
        {
           try
            {
                e.ModelFilePath = Models[e.ModelUri];
            }
            catch (Exception ex)
            {
                 throw new ArgumentException("Cannot locate Nodeset file for Model URI: " + e.ModelUri + " in the CWD: " + Directory.GetCurrentDirectory());
            }
         }

        static bool FileIsGood( string filename )
        {
            if( File.Exists( filename ))
                 return true;

            throw new ArgumentException("Nodeset file '" + filename + "' does not exist in the CWD: " + Directory.GetCurrentDirectory());
        }

        static void ConvertModel( Configuration configuration, 
            string NodesetFile, string outputFile = null)
        {
            if (FileIsGood(NodesetFile))
            {
                Console.WriteLine("Processing " + NodesetFile + " ...");
                Utils.LogInfo( "Processing " + NodesetFile + " ..." );
                ModelManager manager = new ModelManager();
                manager.ModelRequired += ModelImporter_ModelRequired;
                NodeSetToAML convertor = new NodeSetToAML(manager, configuration );
                convertor.CreateAML(NodesetFile, outputFile);
            }
        }
        
        static void ShowSyntax()
        {
            Console.WriteLine("\n++++++++++  Opc2Aml Help  +++++++++++");
            Console.WriteLine("Converts one or more OPC UA Nodeset files in the current working directory (CWD) into their equivalent\nAutomationML Libraries.\n");
            Console.WriteLine("Opc2Aml.exe [NodesetFile] [AmlBaseFilename]\n");
            Console.WriteLine("NodesetFile       Name of nodeset file in the CWD to be processed");
            Console.WriteLine("AmlBaseFilename   File name of the AutomationML file to be written (without the .amlx extension).");
            Console.WriteLine("                  The default name is the NodesetFile if this argument is missing.");
            Console.WriteLine("\nWith no arguments, all nodeset files in CWD are processed.");
            Console.WriteLine("NOTE: All dependent nodeset files need to be present in the CWD, even if they are not processed. ");
            Console.WriteLine("Copyright(c) 2021-2022 OPC Foundation.  All rights reserved.");
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++\n\n");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Opc2Aml ...");

#if (ENABLE_PROGRAM_EXCEPTION)
            try
#endif
            {
                Program program = new Program();

                switch (args.Length)
                {
                    case 0:  // if no args build everything 
                        if (Models.Count == 0)
                        {
                            throw new ArgumentException("Nothing to do -- No Nodeset files found in CWD: " + Directory.GetCurrentDirectory());
                        }
                        foreach (var model in Models)
                        {
                            ConvertModel(program._configuration, model.Value);
                        }
                        break;
                    case 1:
                        ConvertModel( program._configuration, args[0]);
                        break;
                    case 2:
                        ConvertModel( program._configuration, args[0], args[1]);
                        break;
                    default:
                        throw new ArgumentException("wrong number of arguments.");
                        
                } 
                Console.WriteLine("... completed successfully.");
                Utils.LogInfo( "Opc2Aml completed successfully." );
            }

#if ENABLE_PROGRAM_EXCEPTION
            catch (Exception ex)
            {
                Utils.LogError( "** FATAL EXCEPTION ** [{0}]", ex.Message );
                Utils.LogError( "Call Stack " + ex.StackTrace );

                Console.WriteLine("** FATAL EXCEPTION **");
                Console.WriteLine(ex.Message);
                ShowSyntax();           
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
                Environment.Exit(1);
                
            }
#endif    

        }

        public Configuration _configuration = null;
    }
}
