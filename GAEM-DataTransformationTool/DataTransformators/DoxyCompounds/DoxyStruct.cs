using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public class DoxyStruct : DoxyCompound
    {
        public string Fullname { get; protected set; }
        public string Language { get; protected set; }
        public string Location { get; protected set; }
        public List<string> InnerFunctionIds { get; protected set; }

        public DoxyStruct(string id, string language, string location, string name, string fullname, List<string> innerFunctionIds)
        {
            Kind = "struct";
            Id = Pseudonymizer.PseudonymizeString(id);
            Language = language;
            Location = Pseudonymizer.PseudonymizeFilePath(location);
            Name = Pseudonymizer.PseudonymizeString(name);
            Fullname = Pseudonymizer.PseudonymizeHierarchy(fullname);
            InnerFunctionIds = innerFunctionIds.Select(x => Pseudonymizer.PseudonymizeString(x)).ToList();
        }

        public override XElement ToXElement()
        {
            return new XElement("DoxyStruct",
                new XAttribute("Id", Id),
                new XAttribute("Kind", Kind),
                new XAttribute("Name", Name),
                new XAttribute("Fullname", Fullname),
                new XAttribute("Language", Language),
                new XAttribute("Location", Location),
                new XElement("InnerFunctionIds",
                    InnerFunctionIds.Select(x => new XElement("FunctionId", x)))
                );
        }
    }
}
