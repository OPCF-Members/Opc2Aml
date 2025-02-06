/* ========================================================================
 * Copyright (c) 2005-2024 The OPC Foundation, Inc. All rights reserved.
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

using Microsoft.Extensions.Configuration;
using Opc.Ua;
using System.Diagnostics;
using System.Reflection;

namespace Opc2AmlConsole
{
    internal class Program
    {
        static void Main( string[] args )
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( Directory.GetCurrentDirectory() );
            bool directorySpecified = false;
            string configurationFile = "app.config.json";

            string inputNodeset = string.Empty;
            string output = string.Empty;
            bool suppressPrompt = false;
            bool configurationSpecified = false;
            bool insert = false;
            string insertUris = string.Empty;

            if( !String.IsNullOrEmpty( Environment.CommandLine ) )
            {
                string[] parameters = Environment.CommandLine.Split( "--" );

                foreach( string parameter in parameters )
                {
                    if( parameter.Contains( "Opc2AmlConsole.", StringComparison.OrdinalIgnoreCase ) )
                    {
                        continue;
                    }

                    string working = parameter.Trim();
                    string[] parts = working.Split( ' ' );
                    if( parts.Length > 1 )
                    {
                        string id = parts[ 0 ];
                        string value = String.Join( ' ', parts, 1, parts.Length - 1 ).Replace( "\"", "" );

                        Console.WriteLine( id + " [" + value + "]" );

                        if( id.Equals( "DirectoryInfo", StringComparison.OrdinalIgnoreCase ) )
                        {
                            directoryInfo = new DirectoryInfo( value );
                            directorySpecified = true;
                        }
                        else if( id.Equals( "Nodeset", StringComparison.OrdinalIgnoreCase ) )
                        {
                            inputNodeset = value;
                        }
                        else if( id.Equals( "Output", StringComparison.OrdinalIgnoreCase ) )
                        {
                            output = value;
                        }
                        else if( id.Equals( "Config", StringComparison.OrdinalIgnoreCase ) )
                        {
                            configurationFile = value;
                            configurationSpecified = true;
                        }
                        else if( id.Equals( "Insert", StringComparison.OrdinalIgnoreCase ) )
                        {
                            insertUris = value;
                            insert = true;
                        }
                    }
                    else if( parts.Length == 1 )

                    {
                        if ( parts[ 0 ].Equals( "SuppressPrompt", StringComparison.OrdinalIgnoreCase ) )
                        {
                            suppressPrompt = true;
                        }   
                    }
                }
            }

            if( !directoryInfo.Exists )
            {
                Console.WriteLine( "DirectoryInfo does not exist: " + directoryInfo.FullName );
                ShowSyntax(suppressPrompt);
                Environment.Exit( 1 );
            }

            if( output != string.Empty && inputNodeset == string.Empty )
            {
                Console.WriteLine( "NodesetFile must be specified when output is specified." );
                ShowSyntax( suppressPrompt );
                Environment.Exit( 1 );
            }

            FileInfo nodesetFileInfo = null;
            FileInfo outputInfo = null;

            if ( !String.IsNullOrEmpty( inputNodeset ) )
            {
                nodesetFileInfo = new FileInfo( Path.Combine( directoryInfo.FullName, inputNodeset ) );
                if( !nodesetFileInfo.Exists )
                {
                    Console.WriteLine( "NodesetFile does not exist: " + inputNodeset );
                    ShowSyntax( suppressPrompt );
                    Environment.Exit( 1 );
                }
            }

            if( !String.IsNullOrEmpty( output ) )
            {
                if ( Path.IsPathFullyQualified( output ) )
                {
                    outputInfo = new FileInfo( output );
                }
                else
                {
                    outputInfo = new FileInfo( Path.Combine( nodesetFileInfo.Directory.FullName, output ) );
                }
            }

            List<string> insertList = null;
            if ( insert )
            {
                if ( !String.IsNullOrEmpty( insertUris ) )
                {
                    string[] uris = insertUris.Split( ',' );
                    insertList = new List<string>( uris );
                }
                else
                { 
                    Console.WriteLine( "Insert uris not specified" );
                    ShowSyntax( suppressPrompt );
                    Environment.Exit( 1 );
                }
            }

            FileInfo configurationInfo = null;

            if( configurationSpecified )
            {
                if( Path.IsPathFullyQualified( configurationFile ) )
                {
                    configurationInfo = new FileInfo( configurationFile );
                }
                else
                {
                    configurationInfo = new FileInfo( Path.Combine( directoryInfo.FullName, configurationFile ) );
                }
            }
            else
            {
                configurationInfo = new FileInfo( configurationFile );
            }

            Opc2Aml.Entry entry = new Opc2Aml.Entry( directoryInfo, configurationInfo.FullName );

            entry.Run( nodesetFileInfo, insertList, outputInfo, suppressPrompt );
        }

        static void ShowSyntax( bool suppressPrompt )
        {
            Opc2Aml.Entry.ShowSyntax();
            PromptExit( suppressPrompt );
        }

        static void PromptExit(bool suppressPrompt )
        {
            if( !suppressPrompt )
            {
                Console.WriteLine( "\nPress Enter to exit.\n" );
                Console.ReadLine();
            }
        }
    }
}
