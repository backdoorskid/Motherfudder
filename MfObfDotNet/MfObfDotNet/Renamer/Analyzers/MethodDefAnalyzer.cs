using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public class MethodDefAnalyzer : SimpleAnalyzer
    {
        public override bool Execute(object context)
        {
            dnlib.DotNet.MethodDef method = (dnlib.DotNet.MethodDef)context;
            if (method.IsRuntimeSpecialName)
                return false;
            if (method.DeclaringType.IsForwarder)
                return false;
            if (method.DeclaringType.Module.Name.Contains("Costura"))
                return false;
            if (method.DeclaringType.IsAbstract)
                return false;
            return true;
        }
    }
}
