using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace GAEM.DataTransformationTool.DataTransformators
{
    public interface DataTransformer
    {
        void Transform();
    }
}
