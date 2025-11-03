using dnlib.DotNet.Writer;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    internal class Program
    {
        static void ConWrite(string a)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[+] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(a);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1) { return; }
            string filePath = args[0];

            // string filePath = "payload.exe";

            ModuleDefMD module = ModuleDefMD.Load(filePath);
            ConWrite("Loaded module into memory");
            ObfuscateModule(module);

            ModuleWriterOptions moduleWriterOptions = new ModuleWriterOptions(module);
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;

            string outPath = filePath.Replace(".exe", ".obf.exe");
            module.Write(outPath, moduleWriterOptions);
            ConWrite("Obfuscation is complete");
        }

        static void ObfuscateModule(ModuleDefMD module)
        {
            Renamer.Execute(module);
            ConWrite("Renamer.Execute(module)");

            StringObfuscator.Execute(module);
            ConWrite("StringObfuscator.Execute(module)");

            IntegerObfuscator.Execute(module);
            ConWrite("IntegerObfuscator.Execute(module)");

            RandomizeOrder.Execute(module);
            ConWrite("RandomizeOrder.Execute(module)");

            AttributesModifier.Execute(module);
            ConWrite("AttributesModifier.Execute(module)");
        }
    }
}
