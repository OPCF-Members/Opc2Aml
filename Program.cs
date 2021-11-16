using System;
using System.IO;
using System.Collections.Generic;
using MarkdownProcessor;

namespace Opc2Aml
{

    internal class Program
    {
        
        
        public static Dictionary<string, string> Models;
        private Program()
        {
            Models = new Dictionary<string, string>();
            // load the dictionary with the UA models available in the Current Working Directory (CWD)
            string[] fileEntries = Directory.GetFiles("./", "*.xml");
            foreach (string fileEntry in fileEntries)
            {
                ModelManager manager = new ModelManager();
                string uri = manager.LoadModel(fileEntry, null, null);
                if( uri != null)
                    Models.Add(uri, fileEntry.Substring(2));

            }


            

        }

        static void ModelImporter_ModelRequired(object sender, ModelRequiredEventArgs e)
        {
           try
            {
                e.ModelFilePath = Models[e.ModelUri];

            }
            catch (Exception )
            {
                Console.WriteLine("Cannot locate Nodeset file for Model URI: " + e.ModelUri);
                ShowSyntax();
            }
         }

        static bool FileIsGood( string filename )
        {
            if( File.Exists( filename ))
                 return true;

            Console.WriteLine(filename + " Does not exist \n");
            ShowSyntax();
            return false;
        }

        static void ConvertModel( string NodesetFile, string outputFile = null)
        {
            if (FileIsGood(NodesetFile))
            {
                Console.WriteLine("Processing " + NodesetFile);
                ModelManager manager = new ModelManager();
                manager.ModelRequired += ModelImporter_ModelRequired;

                NodeSetToAML convertor = new NodeSetToAML(manager);

                convertor.CreateAML(NodesetFile, outputFile);
            }
        }
        
        static void ShowSyntax()
        {
            Console.WriteLine("Converts one or more OPC UA Nodeset files in the current working directory (CWD) into their equivalent\nAutomationML Libraries.\n");
            Console.WriteLine("\nOpc2Aml.exe [NodesetFile] [AmlBaseFilename]\n");
            Console.WriteLine("NodesetFile       Name of nodeset file in the CWD to be processed");
            Console.WriteLine("AmlBaseFilename   File name of the AutomationML file to be written (without the .amlx extension).");
            Console.WriteLine("                  The default name is the NodesetFile if this argument is missing.");
            Console.WriteLine("\nWith no optional arguments, all nodeset files in CWD are processed.");
            Console.WriteLine("All dependent nodeset files need to be present in the CWD, even if they are not processed. ");
            Console.WriteLine("Copyright(c) 2021 OPC Foundation.  All rights reserved.\n\n");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Opc2Aml ...");
            Program program = new Program();

            switch( args.Length)
            {
                case 0:  // if no args build everything 
                    if( Models.Count == 0)
                    {
                        Console.WriteLine("Nothing to do -- No Nodeset files found in CWD: " + Directory.GetCurrentDirectory());
                        ShowSyntax();
                    }
                    foreach (var model in Models)
                    {
                        ConvertModel(model.Value);
                    }
                    break;
                case 1:
                    ConvertModel(args[0]);
                    break;
                case 2:
                    ConvertModel(args[0], args[1]);
                    break;
                default:
                    ShowSyntax();
                    break;
            }
            Console.WriteLine("... done.   Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
