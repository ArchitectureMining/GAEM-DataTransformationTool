using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GAEM.DataTransformationTool.Pseudonymizers;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public class DoxyRelation : Doxy
    {
        protected Pseudonymizer Pseudonymizer { get; } = GlobalSettings.DoxygenSettings.PseudonymizeDoxygen ? new Pseudonymizer() : new DummyPseudonymizer();

        public string From { get; protected set; }
        public string FromProperty { get; protected set; }
        public string To { get; protected set; }
        public string ToProperty { get; protected set; }

        public DoxyRelation(string fromProperty, string from,string toProperty, string to)
        {
            FromProperty = fromProperty;
            ToProperty = toProperty;
            From = ParsePropertyValue(fromProperty, from);
            To = ParsePropertyValue(toProperty, to);
        }

        public string ToCSVLine()
        {
            return String.Join(",", new[] { From, To });
        }

        private string ParsePropertyValue(string property, string value)
        {
            switch (property)
            {
                case "fullname":
                    return Pseudonymizer.PseudonymizeHierarchy(value);
                case "location":
                    if (value.Last() == '/')
                    {
                        return Pseudonymizer.PseudonymizeDirectoryPath(value);
                    }
                    else
                    {
                        return Pseudonymizer.PseudonymizeFilePath(value);
                    }
                default:
                    throw new NotImplementedException("ERROR: The property type '" + property + "' is not implemented.");
            }
        }
    }
}
