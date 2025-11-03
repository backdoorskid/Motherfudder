using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;

namespace MfObfDotNet
{
    public partial class IntegerObfuscator
    {
        public static Instruction CreateProxyObfLdcI4(ModuleDefMD module, int value)
        {
            var obfInstructions = ConvertIntoObfInstructionsLdcI4(value);
            for (int i = 0; i < obfInstructions.Count; i++)
            {
                if (obfInstructions[i].OpCode.ToString() == "ldc.i4")
                {
                    obfInstructions[i] = CreateProxyLdcI4(module, (int)obfInstructions[i].Operand);
                }
            }

            var m = new MethodDefUser(
                Random.Variable(),
                MethodSig.CreateStatic(module.CorLibTypes.Int32),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
            );

            m.Body = new CilBody();
            foreach (var instr in obfInstructions)
                m.Body.Instructions.Add(instr);
            m.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            IntegerSplitProxyMethods.Add(m);
            return OpCodes.Call.ToInstruction(m);
        }

        public static Instruction CreateProxyLdcI4(ModuleDefMD module, int value)
        {
            var m = new MethodDefUser(
                Random.Variable(),
                MethodSig.CreateStatic(module.CorLibTypes.Int32),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
            );
            
            m.Body = new CilBody();
            m.Body.Variables.Add(new Local(module.CorLibTypes.Int32));
            m.Body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(value));
            m.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

            IntegerSplitProxyMethods.Add(m);
            return OpCodes.Call.ToInstruction(m);
        }

        public static List<Instruction> ConvertIntoObfInstructionsLdcI4(int value)
        {
            List<Instruction> instructions = new List<Instruction>();

            int state = (int)Random.U32();
            instructions.Add(OpCodes.Ldc_I4.ToInstruction(state));

            for (int i = 0; i < Random.U32() % 4 + 1; i++) // 1 - 4
            {
                int randomValue = (int)Random.U32();
                switch (Random.U32() % 3)
                {
                    case 0:
                        {
                            state += randomValue;
                            instructions.Add(OpCodes.Ldc_I4.ToInstruction(randomValue));
                            instructions.Add(OpCodes.Add.ToInstruction());
                            break;
                        }
                    case 1:
                        {
                            state -= randomValue;
                            instructions.Add(OpCodes.Ldc_I4.ToInstruction(randomValue));
                            instructions.Add(OpCodes.Sub.ToInstruction());
                            break;
                        }
                    case 2:
                        {
                            state ^= randomValue;
                            instructions.Add(OpCodes.Ldc_I4.ToInstruction(randomValue));
                            instructions.Add(OpCodes.Xor.ToInstruction());
                            break;
                        }
                }
            }

            switch (Random.U32() % 3)
            {
                case 0:
                    {
                        int addPart = value - state;
                        instructions.Add(OpCodes.Ldc_I4.ToInstruction(addPart));
                        instructions.Add(OpCodes.Add.ToInstruction());
                        break;
                    }
                case 1:
                    {
                        int subPart = state - value;
                        instructions.Add(OpCodes.Ldc_I4.ToInstruction(subPart));
                        instructions.Add(OpCodes.Sub.ToInstruction());
                        break;
                    }
                case 2:
                    {
                        int xorPart = state ^ value;
                        instructions.Add(OpCodes.Ldc_I4.ToInstruction(xorPart));
                        instructions.Add(OpCodes.Xor.ToInstruction());
                        break;
                    }
            }

            return instructions;
        }
    }
}
