using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public partial class IntegerObfuscator
    {
        public static List<MethodDefUser> IntegerSplitProxyMethods = new List<MethodDefUser>();
        public static void Execute(ModuleDefMD module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var meth in type.Methods.ToList())
                {
                    if (!meth.HasBody) continue;
                    meth.Body.SimplifyMacros(meth.Parameters);
                    var instr = meth.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        string opCodeString = meth.Body.Instructions[i].OpCode.ToString();

                        switch (opCodeString)
                        {
                            case "ldc.i4":
                                {
                                    var intProxy = CreateProxyObfLdcI4(module, (int)instr[i].Operand);
                                    meth.Body.Instructions[i].OpCode = intProxy.OpCode;
                                    meth.Body.Instructions[i].Operand = intProxy.Operand;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }

                foreach (var meth in IntegerSplitProxyMethods)
                {
                    type.Methods.Add(meth);
                }
                IntegerSplitProxyMethods.Clear();
            }
        }
    }
}
