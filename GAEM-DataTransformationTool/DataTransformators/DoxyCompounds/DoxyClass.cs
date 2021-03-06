﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public class DoxyClass : DoxyCompound
    {
        public string Fullname { get; protected set; }
        public string Language { get; protected set; }
        public string DeclarationFile { get; protected set; }
        public string BodyFile { get; protected set; }
        public List<DoxyFunction> InnerFunctions { get; protected set; }
        public List<string> InnerStructIds { get; protected set; }

        public DoxyClass(string id, string language, string declarationFile, string bodyFile, string name, string fullname, List<DoxyFunction> innerFunctions, List<string> innerStructIds)
        {
            Kind = "class";
            Id = Pseudonymizer.PseudonymizeString(id);
            Language = language;
            DeclarationFile = Pseudonymizer.PseudonymizeFilePath(declarationFile);
            BodyFile = bodyFile != null ? Pseudonymizer.PseudonymizeFilePath(bodyFile) : bodyFile;
            Name = Pseudonymizer.PseudonymizeString(name);
            Fullname = Pseudonymizer.PseudonymizeHierarchy(fullname);
            InnerFunctions = innerFunctions;
            InnerStructIds = innerStructIds.Select(x => Pseudonymizer.PseudonymizeString(x)).ToList();
        }

        public override string ToCSVLine()
        {
            return String.Join(",", new[] { Name, Fullname, Language, DeclarationFile, BodyFile });
        }

        public override XElement ToXElement()
        {
            return new XElement("DoxyClass",
                new XAttribute("Id", Id),
                new XAttribute("Kind", Kind),
                new XAttribute("Name", Name),
                new XAttribute("Fullname", Fullname),
                new XAttribute("Language", Language),
                new XAttribute("DeclarationFile", DeclarationFile),
                new XAttribute("BodyFile", BodyFile != null ? BodyFile : ""),
                new XElement("InnerFunctions",
                    InnerFunctions.Select(x => x.ToXElement())),
                new XElement("InnerStructIds",
                    InnerStructIds.Select(x => new XElement("StructId", x)))
                );
        }
    }
}
