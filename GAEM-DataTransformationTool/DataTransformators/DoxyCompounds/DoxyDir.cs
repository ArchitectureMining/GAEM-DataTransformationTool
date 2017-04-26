using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public class DoxyDir : DoxyCompound
    {
        public List<string> InnerFileIds { get; protected set; }
        public List<string> InnerDirIds { get; protected set; }
        public string Location { get; protected set; }

        public DoxyDir(string id, string name, string location, List<string> innerFileIds, List<string> innerDirIds)
        {
            Kind = "dir";
            Id = Pseudonymizer.PseudonymizeString(id);
            Name = Pseudonymizer.PseudonymizeString(name);
            Location = Pseudonymizer.PseudonymizeDirectoryPath(location);
            InnerFileIds = innerFileIds.Select(x => Pseudonymizer.PseudonymizeString(x)).ToList();
            InnerDirIds = innerDirIds.Select(x => Pseudonymizer.PseudonymizeString(x)).ToList();
        }

        public override XElement ToXElement()
        {
            return new XElement("DoxyDir",
                new XAttribute("Id", Id),
                new XAttribute("Kind", Kind),
                new XAttribute("Name", Name),
                new XAttribute("Location", Location),
                new XElement("InnerDirIds",
                    InnerDirIds.Select(x => new XElement("DirId", x))),
                new XElement("InnerFileIds",
                    InnerFileIds.Select(x => new XElement("FileId", x)))
                );
        }
    }
}
