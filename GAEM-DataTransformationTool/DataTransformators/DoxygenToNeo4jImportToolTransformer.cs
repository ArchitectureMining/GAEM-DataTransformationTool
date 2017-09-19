using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using GAEM.DataTransformationTool.DataTransformators.DoxyCompounds;

namespace GAEM.DataTransformationTool.DataTransformators
{
    class DoxygenToNeo4jImportToolTransformer : DataTransformer
    {
        public ConcurrentDictionary<string, ImportableCodeContainer> codeContainers = new ConcurrentDictionary<string, ImportableCodeContainer>();
        public ConcurrentDictionary<string, ImportableFile> files = new ConcurrentDictionary<string, ImportableFile>();
        public ConcurrentDictionary<string, ImportableDir> dirs = new ConcurrentDictionary<string, ImportableDir>();

        public void Transform()
        {
            // TODO
            // - copy folder structure before starting to process files ?
            // - use Konsole with active progress feedback
            // - add try catches
            // - add logging
            // - handle not existing directory
            
            BlockingCollection<string> fileCollection = CreateBlockingXMLCollection(GlobalSettings.DoxygenSettings.DoxygenRootDirectory + @"xml/");
            int totalAmountOfFiles = fileCollection.Count();
            int taskCount;
            BlockingCollection<XElement> transformedXElements = new BlockingCollection<XElement>();

            if (totalAmountOfFiles < Environment.ProcessorCount * 2)
            {
                taskCount = totalAmountOfFiles;
            }
            else
            {
                taskCount = Environment.ProcessorCount;
            }

            Task[] taskArray = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                taskArray[i] = Task.Factory.StartNew(() =>
                {
                    string fileName;

                    while (!fileCollection.IsCompleted)
                    {
                        if (!fileCollection.TryTake(out fileName)) continue;

                        HandleXMLFile(fileName);
                    }
                });
            }

            Task.WaitAll(taskArray);

            Directory.CreateDirectory(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/");

            /*File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/classes.csv", csvClasses);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/dirs.csv", csvDirs);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/files.csv", csvFiles);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/functions.csv", csvFunctions);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/namespaces.csv", csvNamespaces);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/structs.csv", csvStructs);

            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/structInClassRelations.csv", csvStructInClassRelations);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/functionInClassRelations.csv", csvFunctionInClassRelations);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/fileInDirRelations.csv", csvFileInDirRelations);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/dirInDirRelations.csv", csvDirInDirRelations);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/classStructInFileRelations.csv", csvClassStructInFileRelations);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/namespaceInFileRelations.csv", csvNamespaceInFileRelations);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/namespaceInNamespaceRelations.csv", csvNamespaceInNamespaceRelations);
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/functionInStructRelations.csv", csvFunctionInStructRelations);*/
        }

        private void HandleXMLFile(string filePath)
        {
            if (Path.GetFileName(filePath) == "index.xml")
            {
                // add to ignored list
                // log that it's ignored
                return;
            }

            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                XDocument doc = XDocument.Load(reader);
                XElement compounddef = doc.Root.Element("compounddef");

                string id = compounddef.Attribute("id").Value;
                string kind = compounddef.Attribute("kind").Value;
                string compoundname = compounddef.Element("compoundname").Value;

                try
                {
                    switch (kind)
                    {
                        case "class":
                            {                                
                                codeContainers[compoundname] = new ImportableCodeContainer()
                                {
                                    Label = kind,
                                    Name = compoundname.Split(new string[] { "::" }, StringSplitOptions.None).Last(),
                                    Fullname = compoundname,
                                    Language = compounddef.Attribute("language").Value
                                };                                                           
                            }
                            break;
                        case "dir":
                            {
                                /*dirs.Add(new ImportableDir()
                                {
                                    Label = kind,
                                    Name = compoundname.Split(new string[] { @"\", "/" }, StringSplitOptions.None).Last(),
                                    Location = compounddef.Element("location").Attribute("file").Value
                                });*/
                            }
                            break;
                        case "example":
                            // ignore
                            break;
                        case "group":
                            //ignore
                            break;
                        case "file":
                            {
                                /*files.Add(new ImportableFile()
                                {
                                    Label = kind,
                                    Name = compoundname,
                                    Location = compounddef.Element("location").Attribute("file").Value,
                                    Language = compounddef.Attribute("language").Value
                                });*/
                            }
                            break;
                        case "interface":
                            // ignore
                            break;
                        case "namespace":
                            {
                                
                            }
                            break;
                        case "page":
                            // ignore
                            break;
                        case "struct":
                            {
                                
                            }
                            break;
                        case "union":
                            // ignore
                            break;
                        default:
                            throw new NotImplementedException("ERROR | Not implemented kind = " + kind);
                    }
                }
                catch (Exception e)
                {
                    Console.Write("fullname: " + compoundname);
                    throw e;
                }
            }
        }

        private BlockingCollection<string> CreateBlockingXMLCollection(string path)
        {
            var allFiles = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
            var filePaths = new BlockingCollection<string>();
            foreach (var fileName in allFiles)
            {
                filePaths.Add(fileName);
            }
            filePaths.CompleteAdding();
            return filePaths;
        }

        public interface Importable
        {
            string ToString();
        }

        public class ImportableCodeContainer : Importable
        {
            public string Label { get; set; }
            public string Name { get; set; }
            public string Fullname { get; set; }
            public string Language { get; set; }
            public override string ToString()
            {
                return String.Join(",", new[] { Label, Name, Fullname, Language });
            }
        }

        public class ImportableFile : Importable
        {
            public string Label { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public string Language { get; set; }

            public override string ToString()
            {
                return String.Join(",", new[] { Label, Name, Location, Language });
            }
        }

        public class ImportableDir : Importable
        {
            public string Label { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }

            public override string ToString()
            {
                return String.Join(",", new[] { Label, Name, Location});
            }
        }
    }
}
