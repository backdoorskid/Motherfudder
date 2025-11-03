using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public partial class StringObfuscator
    {
        public static List<MethodDefUser> StringSplitProxyMethods = new List<MethodDefUser>();
        public static void Execute(ModuleDefMD module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (var meth in type.Methods.ToList())
                {
                    if (!meth.HasBody) continue;
                    var instr = meth.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        string opCodeString = meth.Body.Instructions[i].OpCode.ToString();

                        switch (opCodeString)
                        {
                            case "ldstr":
                                {
                                    var strProxy = CreateEncProxyObfLdstr(module, type, (string)instr[i].Operand);
                                    meth.Body.Instructions[i].OpCode = strProxy.OpCode;
                                    meth.Body.Instructions[i].Operand = strProxy.Operand;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }

                foreach (var meth in StringSplitProxyMethods)
                {
                    type.Methods.Add(meth);
                }
                StringSplitProxyMethods.Clear();
            }
        }
    }
}
