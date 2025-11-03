using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public partial class StringObfuscator
    {
        public static DecryptionMethod decMethod;
        public static bool shouldAddMethod = true;
        public static Instruction CreateEncProxyObfLdstr(ModuleDefMD module, TypeDef type, string value)
        {
            if (shouldAddMethod)
            {
                decMethod = GenerateDecryptionFunction(module);
                // I don't know if a private class might break it all but I'm not worrying about it right now seen as the stub is just one class.
                type.Methods.Add(decMethod.decryptionMethod);
                shouldAddMethod = false;
            }

            string key = Random.String();
            string encryptedString = EncryptString(decMethod, value, key);

            var concatProxyMethodsList = SplitStringIntoConcatProxyMethods(module, encryptedString);
            var stringConcatMethod = module.Import(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));

            var m = new MethodDefUser(
                Random.Variable(),
                MethodSig.CreateStatic(module.CorLibTypes.String),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
            );

            m.Body = new CilBody();
            m.Body.Instructions.Add(OpCodes.Call.ToInstruction(concatProxyMethodsList[0]));
            concatProxyMethodsList.Remove(concatProxyMethodsList[0]);
            foreach (var p in concatProxyMethodsList)
            {
                m.Body.Instructions.Add(OpCodes.Call.ToInstruction(p));
                m.Body.Instructions.Add(OpCodes.Call.ToInstruction(stringConcatMethod));
            }
            m.Body.Instructions.Add(OpCodes.Ldstr.ToInstruction(key));
            m.Body.Instructions.Add(OpCodes.Call.ToInstruction(decMethod.decryptionMethod));
            m.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            StringSplitProxyMethods.Add(m);

            return OpCodes.Call.ToInstruction(m);
        }

        public static List<MethodDefUser> SplitStringIntoConcatProxyMethods(ModuleDefMD module, string value)
        {
            List<MethodDefUser> result = new List<MethodDefUser>();

            int t = 0;
            int md = value.Length > 100 ? 80 : (value.Length > 30 ? 24 : 5);

            if (value.Length == 0)
            {
                var m = new MethodDefUser(
                    Random.Variable(),
                    MethodSig.CreateStatic(module.CorLibTypes.String),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
                );

                m.Body = new CilBody();
                m.Body.Variables.Add(new Local(module.CorLibTypes.String));
                m.Body.Instructions.Add(OpCodes.Ldstr.ToInstruction(""));
                m.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

                StringSplitProxyMethods.Add(m);
                result.Add(m);

                return result;
            }

            while (t != value.Length) {
                int dif = value.Length - t;
                int partLength = (int)((Math.Abs(Random.U32()) % md) + 1);
                if (partLength > dif)
                    partLength = dif;

                var part = value.Substring(t, partLength);

                var m = new MethodDefUser(
                    Random.Variable(),
                    MethodSig.CreateStatic(module.CorLibTypes.String),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
                );

                m.Body = new CilBody();
                m.Body.Variables.Add(new Local(module.CorLibTypes.String));
                m.Body.Instructions.Add(OpCodes.Ldstr.ToInstruction(part));
                m.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

                StringSplitProxyMethods.Add(m);
                result.Add(m);

                t += partLength;
            }

            return result;
        }
    }
}
