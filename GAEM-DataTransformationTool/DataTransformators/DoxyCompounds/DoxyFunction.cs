using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public class DoxyFunction : DoxyCompound
    {
        public string Fullname { get; protected set; }
        public string DeclarationFile { get; protected set; }
        public string BodyFile { get; protected set; }

        public DoxyFunction(string id, string declarationFile, string bodyFile, string name, string fullname)
        {
            Kind = "function";
            Id = Pseudonymizer.PseudonymizeString(id);
            DeclarationFile = Pseudonymizer.PseudonymizeFilePath(declarationFile);
            BodyFile = bodyFile != null ? Pseudonymizer.PseudonymizeFilePath(bodyFile) : bodyFile;
            Name = Pseudonymizer.PseudonymizeString(name);
            Fullname = Pseudonymizer.PseudonymizeHierarchy(fullname);
        }

        public override string ToCSVLine()
        {
            return String.Join(",", new[] { Name, Fullname, DeclarationFile, BodyFile });
        }

        public override XElement ToXElement()
        {
            return new XElement("DoxyFunction",
                new XAttribute("Id", Id),
                new XAttribute("Kind", Kind),
                new XAttribute("Name", Name),
                new XAttribute("Fullname", Fullname),
                new XAttribute("DeclarationFile", DeclarationFile),
                new XAttribute("BodyFile", BodyFile != null ? BodyFile : "")
                );
        }
    }
}
