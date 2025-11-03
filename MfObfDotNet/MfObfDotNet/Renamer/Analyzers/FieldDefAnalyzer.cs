using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public class FieldDefAnalyzer : SimpleAnalyzer
    {
        public override bool Execute(object context)
        {
            dnlib.DotNet.FieldDef field = (dnlib.DotNet.FieldDef)context;
            if (field.IsRuntimeSpecialName)
                return false;
            if (field.IsLiteral && field.DeclaringType.IsEnum)
                return false;
            return true;
        }
    }
}
