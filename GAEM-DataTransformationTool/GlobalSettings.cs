using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GAEM.DataTransformationTool
{
    // Thread-safe single instance of settings
    public static class GlobalSettings
    {
        public static GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
        public static ETWSettings ETWSettings { get; set; } = new ETWSettings();
        public static DoxygenSettings DoxygenSettings { get; set; } = new DoxygenSettings();
        
        public static string SettingsFile { get; set; }
        public static string LogDirectory { get; set; }

        public static void WriteSettings(string path)
        {
            // TODO
            // - check if settings exist and/or are valid

            Settings settings = new Settings();
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, settings);
            }

            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | INFO | Settings written to file.");
        }

        public static void ReadSettings(string path)
        {
            // TODO
            // - check for valid file paths ?
            // - check for valid settings file

            Settings settings;
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            using (XmlReader reader = XmlReader.Create(path))
            {
                settings = (Settings)serializer.Deserialize(reader);
            }

            GeneralSettings = settings.GeneralSettings;
            ETWSettings = settings.ETWSettings;
            DoxygenSettings = settings.DoxygenSettings;

            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | INFO | Settings read from file.");
        }

        // For debug purpose
        public static void PrintSettingsToConsole()
        {
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | PathPrefix: " + GeneralSettings.PathPrefix);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | OutputDirectory: " + GeneralSettings.OutputDirectory);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | PseudonymizationSalt: " + GeneralSettings.PseudonymizationSalt);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | ProcessETL: " + ETWSettings.ProcessETL);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | ETLRootDirectory: " + ETWSettings.ETLRootDirectory);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | ETLManifestFile: " + ETWSettings.ETLManifestFile);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | PseudonymizeETL: " + ETWSettings.PseudonymizeETL);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | ProcessDoxygen: " + DoxygenSettings.ProcessDoxygen);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | DoxygenRootDirectory: " + DoxygenSettings.DoxygenRootDirectory);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | PseudonymizeDoxygen: " + DoxygenSettings.PseudonymizeDoxygen);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | SettingsFile: " + SettingsFile);
            Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff") + " | DEBUG | LogDirectory: " + LogDirectory);
        }
    }


    public class GeneralSettings
    {
        public string PathPrefix { get; set; }
        public string OutputDirectory { get; set; }
        public string PseudonymizationSalt { get; set; }
    }

    public class ETWSettings
    {
        public bool ProcessETL { get; set; }
        public string ETLRootDirectory { get; set; }
        public string ETLManifestFile { get; set; }
        public bool PseudonymizeETL { get; set; }
    }

    public class DoxygenSettings
    {
        public bool ProcessDoxygen { get; set; }
        public string DoxygenRootDirectory { get; set; }
        public bool PseudonymizeDoxygen { get; set; }
    }

    // Nonstatic GlobalSettings for settings parsing
    public class Settings
    {
        public GeneralSettings GeneralSettings { get; set; } = GlobalSettings.GeneralSettings;
        public ETWSettings ETWSettings { get; set; } = GlobalSettings.ETWSettings;
        public DoxygenSettings DoxygenSettings { get; set; } = GlobalSettings.DoxygenSettings;
    }
}
