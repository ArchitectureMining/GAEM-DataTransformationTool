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
    public class DoxygenCSVDataTransformer : DataTransformer
    {
        public BlockingCollection<string> csvClasses { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvDirs { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvFiles { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvFunctions { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvNamespaces { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvStructs { get; set; } = new BlockingCollection<string>();

        public BlockingCollection<string> csvStructInClassRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvFunctionInClassRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvFileInDirRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvDirInDirRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvClassStructInFileRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvNamespaceInFileRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvClassInNamespaceRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvNamespaceInNamespaceRelations { get; set; } = new BlockingCollection<string>();
        public BlockingCollection<string> csvFunctionInStructRelations { get; set; } = new BlockingCollection<string>();


        public void Transform()
        {
            // TODO
            // - copy folder structure before starting to process files ?
            // - use Konsole with active progress feedback
            // - add try catches
            // - add logging
            // - handle not existing directory

            csvClasses.Add(String.Join(",", new[] { "name", "fullname", "language", "declarationfile", "bodyfile" }));
            csvDirs.Add(String.Join(",", new[] { "name", "location" }));
            csvFiles.Add(String.Join(",", new[] { "name", "location", "language" }));
            csvFunctions.Add(String.Join(",", new[] { "name", "fullname", "declarationfile", "bodyfile" }));
            csvNamespaces.Add(String.Join(",", new[] { "name", "fullname", "language" }));
            csvStructs.Add(String.Join(",", new[] { "name", "fullname", "language", "declarationfile", "bodyfile" }));

            csvStructInClassRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvFunctionInClassRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvFileInDirRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvDirInDirRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvClassStructInFileRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvNamespaceInFileRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvClassInNamespaceRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvNamespaceInNamespaceRelations.Add(String.Join(",", new[] { "from", "to" }));
            csvFunctionInStructRelations.Add(String.Join(",", new[] { "from", "to" }));

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

            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/classes.csv", csvClasses);
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
            File.WriteAllLines(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/functionInStructRelations.csv", csvFunctionInStructRelations);
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
                                var temp = new DoxyClass(
                                    id,
                                    compounddef.Attribute("language").Value,
                                    compounddef.Element("location").Attribute("file").Value,
                                    compounddef.Element("location").Attribute("bodyfile") != null ? compounddef.Element("location").Attribute("bodyfile").Value : null,
                                    compoundname.Split(new string[] { "::" }, StringSplitOptions.None).Last(),
                                    compoundname,
                                    compounddef
                                        .Elements("sectiondef")
                                        .Elements("memberdef")
                                        .Where(x => x.Attribute("kind").Value == "function")
                                        .Select(x => new DoxyFunction(
                                            x.Attribute("id").Value,
                                            x.Element("location").Attribute("file").Value,
                                            x.Element("location").Attribute("bodyfile") != null ? x.Element("location").Attribute("bodyfile").Value : null,
                                            x.Element("name").Value,
                                            x.Element("definition").Value.Split(new char[] { ' ' }).Last()))
                                        .ToList(),
                                    compounddef
                                        .Elements("innerclass")
                                        .Select(x => x.Attribute("refid").Value)
                                        .ToList());

                                // add class
                                csvClasses.Add(temp.ToCSVLine());

                                // add struct relations
                                compounddef
                                    .Elements("innerclass")
                                    .Select(x => new DoxyRelation(
                                        "fullname",
                                        x.Value,
                                        "fullname",
                                        compoundname))
                                    .ToList()
                                    .ForEach(x => csvStructInClassRelations.Add(x.ToCSVLine()));

                                // add functions
                                temp.InnerFunctions.ForEach(x => csvFunctions.Add(x.ToCSVLine()));

                                // add function relations
                                temp.InnerFunctions
                                    .Select(x => new DoxyRelation(
                                        "fullname",
                                        x.Fullname,
                                        "fullname",
                                        compoundname))
                                    .ToList()
                                    .ForEach(x => csvFunctionInClassRelations.Add(x.ToCSVLine()));
                            }
                            break;
                        case "dir":
                            {
                                var temp = new DoxyDir(
                                    id,
                                    compoundname.Split(new string[] { @"\", "/" }, StringSplitOptions.None).Last(),
                                    compounddef.Element("location").Attribute("file").Value,
                                    compounddef
                                        .Elements("innerfile")
                                        .Select(x => x.Attribute("refid").Value)
                                        .ToList(),
                                    compounddef
                                        .Elements("innerdir")
                                        .Select(x => x.Attribute("refid").Value)
                                        .ToList());

                                // add dir
                                csvDirs.Add(temp.ToCSVLine());

                                // add file relations
                                compounddef
                                    .Elements("innerfile")
                                    .Select(x => new DoxyRelation(
                                        "location",
                                        compoundname + "/" + x.Value,
                                        "location",
                                        compounddef.Element("location").Attribute("file").Value))
                                    .ToList()
                                    .ForEach(x => csvFileInDirRelations.Add(x.ToCSVLine()));

                                // add dir relations
                                compounddef
                                    .Elements("innerdir")
                                    .Select(x => new DoxyRelation(
                                        "location",
                                        compoundname + "/",
                                        "location",
                                        compounddef.Element("location").Attribute("file").Value))
                                    .ToList()
                                    .ForEach(x => csvDirInDirRelations.Add(x.ToCSVLine()));
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
                                var temp = new DoxyFile(
                                    id,
                                    compounddef.Attribute("language").Value,
                                    compoundname,
                                    compounddef.Element("location").Attribute("file").Value,
                                    compounddef
                                        .Elements("innerclass")
                                        .Select(x => x.Attribute("refid").Value)
                                        .ToList());

                                // add file
                                csvFiles.Add(temp.ToCSVLine());

                                // add class/struct relations
                                compounddef
                                    .Elements("innerclass")
                                    .Select(x => new DoxyRelation(
                                        "fullname",
                                        x.Value,
                                        "location",
                                        compounddef.Element("location").Attribute("file").Value))
                                    .ToList()
                                    .ForEach(x => csvClassStructInFileRelations.Add(x.ToCSVLine()));

                                // add namespace relations
                                compounddef
                                    .Elements("innernamespace")
                                    .Select(x => new DoxyRelation(
                                        "fullname",
                                        x.Value,
                                        "location",
                                        compounddef.Element("location").Attribute("file").Value))
                                    .ToList()
                                    .ForEach(x => csvNamespaceInFileRelations.Add(x.ToCSVLine()));
                            }
                            break;
                        case "interface":
                            // ignore
                            break;
                        case "namespace":
                            {
                                var temp = new DoxyNamespace(
                                    id,
                                    compounddef.Attribute("language").Value,
                                    compoundname.Split(new string[] { "::" }, StringSplitOptions.None).Last(),
                                    compoundname,
                                    compounddef
                                        .Elements("innernamespace")
                                        .Select(x => x.Attribute("refid").Value)
                                        .ToList(),
                                    compounddef
                                        .Elements("innerclas")
                                        .Select(x => x.Attribute("refid").Value)
                                        .ToList());

                                // add namespace
                                csvNamespaces.Add(temp.ToCSVLine());

                                // add class relations
                                compounddef
                                    .Elements("innerclass")
                                    .Select(x => new DoxyRelation(
                                        "fullname",
                                        x.Value,
                                        "fullname",
                                        compoundname))
                                    .ToList()
                                    .ForEach(x => csvClassInNamespaceRelations.Add(x.ToCSVLine()));

                                // add namespace relations
                                compounddef
                                    .Elements("innernamespace")
                                    .Select(x => new DoxyRelation(
                                        "fullname",
                                        x.Value,
                                        "fullname",
                                        compoundname))
                                    .ToList()
                                    .ForEach(x => csvNamespaceInNamespaceRelations.Add(x.ToCSVLine()));
                            }
                            break;
                        case "page":
                            // ignore
                            break;
                        case "struct":
                            {
                                var temp = new DoxyStruct(
                                    id,
                                    compounddef.Attribute("language").Value,
                                    compounddef.Element("location").Attribute("file").Value,
                                    compounddef.Element("location").Attribute("bodyfile") != null ? compounddef.Element("location").Attribute("bodyfile").Value : null,
                                    compoundname.Split(new string[] { "::" }, StringSplitOptions.None).Last(),
                                    compoundname,
                                    compounddef
                                        .Elements("sectiondef")
                                        .Elements("memberdef")
                                        .Where(x => x.Attribute("kind").Value == "function")
                                        .Select(x => new DoxyFunction(
                                            x.Attribute("id").Value,
                                            x.Element("location").Attribute("file").Value,
                                            x.Element("location").Attribute("bodyfile") != null ? x.Element("location").Attribute("bodyfile").Value : null,
                                            x.Element("name").Value,
                                            x.Element("definition").Value.Split(new char[] { ' ' }).Last()))
                                        .ToList());

                                // add struct
                                csvStructs.Add(temp.ToCSVLine());

                                // add functions
                                temp.InnerFunctions.ForEach(x => csvFunctions.Add(x.ToCSVLine()));

                                // add function relations
                                temp.InnerFunctions
                                    .Select(x => new DoxyRelation(
                                        "fullname",
                                        x.Fullname,
                                        "fullname",
                                        compoundname))
                                    .ToList()
                                    .ForEach(x => csvFunctionInStructRelations.Add(x.ToCSVLine()));
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
    }
}
