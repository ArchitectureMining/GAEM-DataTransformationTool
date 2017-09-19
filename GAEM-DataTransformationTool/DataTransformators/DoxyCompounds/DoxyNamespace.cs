using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public class DoxyNamespace : DoxyCompound
    {
        public string Fullname { get; protected set; }
        public string Language { get; protected set; }
        public List<string> InnerNamespaceIds { get; protected set; }
        public List<string> InnerClassIds { get; protected set; }

        public DoxyNamespace(string id, string language, string name, string fullname, List<string> innerNamespaceIds, List<string> innerClassIds)
        {
            Kind = "namespace";
            Id = Pseudonymizer.PseudonymizeString(id);
            Language = language;
            Name = Pseudonymizer.PseudonymizeString(name);
            Fullname = Pseudonymizer.PseudonymizeHierarchy(fullname);
            InnerNamespaceIds = innerNamespaceIds.Select(x => Pseudonymizer.PseudonymizeString(x)).ToList();
            InnerClassIds = innerClassIds.Select(x => Pseudonymizer.PseudonymizeString(x)).ToList();
        }

        public override string ToCSVLine()
        {
            return String.Join(",", new[] { Name, Fullname, Language });
        }

        public override XElement ToXElement()
        {
            return new XElement("DoxyNamespace",
                new XAttribute("Id", Id),
                new XAttribute("Kind", Kind),
                new XAttribute("Name", Name),
                new XAttribute("Fullname", Fullname),
                new XAttribute("Language", Language),
                new XElement("InnerClassIds",
                    InnerClassIds.Select(x => new XElement("ClassId", x))),
                new XElement("InnerNamespaceIds",
                    InnerNamespaceIds.Select(x => new XElement("NamespaceId", x)))
                );
        }
    }
}
