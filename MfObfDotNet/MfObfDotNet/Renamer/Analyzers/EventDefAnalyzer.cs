using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public class EventDefAnalyzer : SimpleAnalyzer
    {
        public bool Execute(object context)
        {
            dnlib.DotNet.EventDef ev = (dnlib.DotNet.EventDef)context;
            if (ev.IsRuntimeSpecialName)
                return false;
            return true;
        }
    }
}
