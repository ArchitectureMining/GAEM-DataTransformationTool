﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public class DoxyFile : DoxyCompound
    {
        public string Language { get; protected set; }
        public string Location { get; protected set; }
        public List<string> InnerClassIds { get; protected set; }

        public DoxyFile (string id, string language, string name, string location, List<string> innerClassIds)
        {
            Kind = "file";
            Id = Pseudonymizer.PseudonymizeString(id);
            Language = language;
            Name = Pseudonymizer.PseudonymizeFilePath(name);
            Location = Pseudonymizer.PseudonymizeFilePath(location);
            InnerClassIds = innerClassIds.Select(x => Pseudonymizer.PseudonymizeString(x)).ToList();
        }

        public override string ToCSVLine()
        {
            return String.Join(",", new[] { Name, Location, Language });
        }

        public override XElement ToXElement()
        {
            return new XElement("DoxyFile",
                new XAttribute("Id", Id),
                new XAttribute("Kind", Kind),
                new XAttribute("Name", Name),
                new XAttribute("Location", Location),
                new XAttribute("Language", Language),
                new XElement("InnerClassIds",
                    InnerClassIds.Select(x => new XElement("ClassId", x)))
                );
        }
    }
}
