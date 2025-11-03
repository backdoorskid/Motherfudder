using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public abstract class SimpleAnalyzer
    {
        public virtual bool Execute(object context)
        {
            return false;
        }
    }
}
