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
    public class DoxygenDataTransformer : DataTransformer
    {
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

                        HandleXMLFile(fileName, ref transformedXElements);
                    }
                });
            }

            Task.WaitAll(taskArray);

            XDocument newDoc = new XDocument(new XElement("DoxyStructureTransformed",
                transformedXElements.Select(x => x))
                );

            Directory.CreateDirectory(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/");

            newDoc.Save(GlobalSettings.GeneralSettings.OutputDirectory + "doxygen/DoxyStructureTransformed.xml");
        }

        private void HandleXMLFile(string filePath, ref BlockingCollection<XElement> transformedXElements)
        {
            HashAlgorithm hashAlgorithm = SHA256.Create();

            if (Path.GetFileName(filePath) == "index.xml")
            {
                // add to ignored list
                // log that it's ignored
                return;
            }

            // else
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                XDocument doc = XDocument.Load(reader);
                XElement compounddef = doc.Root.Element("compounddef");

                string id = compounddef.Attribute("id").Value;
                string kind = compounddef.Attribute("kind").Value;
                string compoundname = compounddef.Element("compoundname").Value;

                DoxyCompound compound = null;
                switch (kind)
                {
                    case "class":
                        {
                            compound = new DoxyClass(id,
                                compounddef.Attribute("language").Value,
                                compounddef.Element("location").Attribute("file").Value,
                                compoundname.Split(new string[] { "::" }, StringSplitOptions.None).Last(),
                                compoundname,
                                compounddef
                                    .Elements("sectiondef")
                                    .Elements("memberdef")
                                    .Where(x => x.Attribute("kind").Value == "function")
                                    .Select(x => x.Attribute("id").Value)
                                    .ToList());
                        }
                        break;
                    case "dir":
                        {
                            compound = new DoxyDir(id,
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
                            compound = new DoxyFile(id,
                                compounddef.Attribute("language").Value,
                                compoundname,
                                compounddef.Element("location").Attribute("file").Value);
                        }
                        break;
                    case "interface":
                        // ignore
                        break;
                    case "namespace":
                        {
                            compound = new DoxyNamespace(id,
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
                        }
                        break;
                    case "page":
                        // ignore
                        break;
                    case "struct":
                        {
                            compound = new DoxyStruct(id,
                                compounddef.Attribute("language").Value,
                                compounddef.Element("location").Attribute("file").Value,
                                compoundname.Split(new string[] { "::" }, StringSplitOptions.None).Last(),
                                compoundname,
                                compounddef
                                    .Elements("sectiondef")
                                    .Elements("memberdef")
                                    .Where(x => x.Attribute("kind").Value == "function")
                                    .Select(x => x.Attribute("id").Value)
                                    .ToList());
                        }
                        break;
                    case "union":
                        // ignore
                        break;
                    default:
                        throw new NotImplementedException("ERROR | Not implemented kind = " + kind);
                }
                if (compound != null)
                {
                    transformedXElements.Add(compound.ToXElement());
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
