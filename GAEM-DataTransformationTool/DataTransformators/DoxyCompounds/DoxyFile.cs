using System;
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

        public DoxyFile (string id, string language, string name, string location)
        {
            Kind = "file";
            Id = Pseudonymizer.PseudonymizeString(id);
            Language = language;
            Name = Pseudonymizer.PseudonymizeFilePath(name);
            Location = Pseudonymizer.PseudonymizeFilePath(location);
        }

        public override XElement ToXElement()
        {
            return new XElement("DoxyFile",
                new XAttribute("Id", Id),
                new XAttribute("Kind", Kind),
                new XAttribute("Name", Name),
                new XAttribute("Location", Location),
                new XAttribute("Language", Language)
                );
        }
    }
}
