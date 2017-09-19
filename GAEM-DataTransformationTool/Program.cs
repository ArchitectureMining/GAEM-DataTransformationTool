using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CommandLine;
using CommandLine.Text;
using Konsole;
using GAEM.DataTransformationTool.DataTransformators;

// TODO
// - Start using Konsole and/or Nlog for reporting
// - build decent try catch constructions where necessary

namespace GAEM.DataTransformationTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // process commandline options
            var commandLineOptions = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => {
                    GlobalSettings.SettingsFile = options.SettingsFile;
                    GlobalSettings.LogDirectory = options.LogDirectory;
                    GlobalSettings.ReadSettings(options.SettingsFile);
                    })
                .WithNotParsed(errors => Environment.Exit(1));

            // DEBUG
            // GlobalSettings.PrintSettingsToConsole();

            // process ETW log files
            if (GlobalSettings.ETWSettings.ProcessETL)
            {
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | INFO | Starting to transform ETW logging data.");
                new ETWDataTransformer().Transform();
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | INFO | Finished transforming ETW logging data.");
            }

            // process Doxygen xml files
            if (GlobalSettings.DoxygenSettings.ProcessDoxygen)
            {
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | INFO | Starting to transform Doxygen data.");
                new DoxygenCSVDataTransformer().Transform();
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | INFO | Finished transforming Doxygen data.");
            }
            
            // Wrap up

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }

    // CommandLineParser Options
    class Options
    {
        [Option('s', "settings",
            Required = true,
            HelpText = "Path to XML settings file to use.")]
        public string SettingsFile { get; set; }

        [Option('l', "logging",
            HelpText = "Path to the directory to write the logs to, disables logging is disabled if not set.")]
        public string LogDirectory { get; set; }
    }
}
