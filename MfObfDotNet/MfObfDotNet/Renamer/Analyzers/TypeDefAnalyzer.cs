using System;

namespace MfObfDotNet
{
    public class TypeDefAnalyzer : SimpleAnalyzer
    {
        public override bool Execute(object context)
        {
            dnlib.DotNet.TypeDef type = (dnlib.DotNet.TypeDef)context;
            if (type.IsRuntimeSpecialName)
                return false;
            if (type.IsGlobalModuleType)
                return false;
            return true;
        }
    }
}
