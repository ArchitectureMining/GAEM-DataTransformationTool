using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


using GAEM.DataTransformationTool.Pseudonymizers;

namespace GAEM.DataTransformationTool.DataTransformators.DoxyCompounds
{
    public abstract class DoxyCompound
    {
        public string Name { get; protected set; }
        public string Id { get; protected set; }
        public string Kind { get; protected set; }

        protected Pseudonymizer Pseudonymizer { get; } = GlobalSettings.DoxygenSettings.PseudonymizeDoxygen ? new Pseudonymizer() : new DummyPseudonymizer();

        //public abstract string ToString();
        public abstract XElement ToXElement();
    }
}
