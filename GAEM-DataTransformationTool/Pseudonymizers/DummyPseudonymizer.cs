using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAEM.DataTransformationTool.Pseudonymizers
{
    public class DummyPseudonymizer : Pseudonymizer
    {
        public override string PseudonymizeString(string input)
        {
            return input;
        }
    }
}
