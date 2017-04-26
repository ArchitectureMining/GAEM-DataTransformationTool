using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;

using GAEM.DataTransformationTool.Pseudonymizers;

namespace GAEM.DataTransformationTool.DataTransformators
{
    public class ETWDataTransformer : DataTransformer
    {
        public void Transform()
        {
            // TODO
            // - copy folder structure before starting to process files -> sorta done, doing it while processing
            // - use Konsole with active progress feedback 
            //     - add verbosity option ?
            // - add try-catch
            // - add logging
            // - handle not existing directory

            BlockingCollection<string> fileCollection = CreateBlockingETLCollection(GlobalSettings.ETWSettings.ETLRootDirectory);
            int totalAmountOfFiles = fileCollection.Count();
            int taskCount;

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

                        HandleETLFile(fileName);
                    }
                });
            }

            Task.WaitAll(taskArray);
        }

        private void HandleETLFile(string fileName)
        {
            Pseudonymizer Pseudonymizer = GlobalSettings.ETWSettings.PseudonymizeETL ? new Pseudonymizer() : new DummyPseudonymizer();

            using (ETWTraceEventSource source = new ETWTraceEventSource(fileName, TraceEventSourceType.FileOnly))
            {
                string outputFileName;
                {
                    string[] splitFileName = fileName
                        .Replace("regression", "regression-logs")
                        .Replace("plain", "pseudonymized") // maybe make it more flexible? i.e. replace settings.input by settings.output ?
                        .Split(new string[] { @"\", "/" }, StringSplitOptions.None);
                    string name = splitFileName[splitFileName.Length - 1];
                    string hashedName = Pseudonymizer.PseudonymizeString(name.Substring(0, name.LastIndexOf('.')));
                    splitFileName[splitFileName.Length - 1] = hashedName + ".csv";
                    outputFileName = string.Join(@"/", splitFileName);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));

                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    DynamicTraceEventParser parser = new DynamicTraceEventParser(source);
                    parser.ReadAllManifests(Path.GetDirectoryName(GlobalSettings.ETWSettings.ETLManifestFile));

                    parser.All += (etlEvent) =>
                    {
                        string eventType;
                        if (etlEvent.OpcodeName == "FunctionBegin")
                        {
                            eventType = "FunctionBegin";
                        }
                        else if (etlEvent.OpcodeName == "FunctionEnd")
                        {
                            eventType = "FunctionEnd";
                        }
                        else
                        {
                            throw new Exception("opcode unknown");
                        }

                        long timestamp = etlEvent.TimeStamp.Ticks;
                        string parsedFileName = Pseudonymizer.PseudonymizeFilePath(etlEvent.PayloadByName("FileName").ToString());
                        string parsedFunctionName = Pseudonymizer.PseudonymizeFUNCSIG(etlEvent.PayloadByName("FunctionName").ToString());

                        writer.WriteLine(string.Join(",", new string[4] {
                            eventType,
                            timestamp.ToString(),
                            parsedFileName,
                            parsedFunctionName }));
                    };

                    source.Process();
                }
            }
        }

        private BlockingCollection<string> CreateBlockingETLCollection(string path)
        {
            var allFiles = Directory.GetFiles(path, "*.etl", SearchOption.AllDirectories);
            var filePaths = new BlockingCollection<string>(allFiles.Count());
            foreach (var fileName in allFiles)
            {
                filePaths.Add(fileName);
            }
            filePaths.CompleteAdding();
            return filePaths;
        }
    }
}
