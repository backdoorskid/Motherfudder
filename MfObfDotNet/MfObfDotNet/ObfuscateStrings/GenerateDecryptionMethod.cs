using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Security.Cryptography;

namespace MfObfDotNet
{
    public partial class StringObfuscator
    {
        public static string EncryptString(DecryptionMethod method, string data, string key)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if (method.encryptionAlgorithm == EmEncryptAlgorithm.AES)
            {
                var csp = new AesCryptoServiceProvider();
                csp.Key = SHA256.Create().ComputeHash(keyBytes);
                csp.Padding = PaddingMode.PKCS7;
                if (method.cipherMode == EmCipherMode.CBC) {
                    csp.Mode = CipherMode.CBC;
                    csp.IV = MD5.Create().ComputeHash(csp.Key);
                } else
                {
                    csp.Mode = CipherMode.ECB;
                }
                return Convert.ToBase64String(csp.CreateEncryptor().TransformFinalBlock(dataBytes, 0, dataBytes.Length));
            } else
            {
                var csp = new TripleDESCryptoServiceProvider();
                csp.Key = MD5.Create().ComputeHash(keyBytes);
                csp.Padding = PaddingMode.PKCS7;
                csp.Mode = CipherMode.ECB;
                return Convert.ToBase64String(csp.CreateEncryptor().TransformFinalBlock(dataBytes, 0, dataBytes.Length));
            }
        }

        public static DecryptionMethod GenerateDecryptionFunction(ModuleDefMD module)
        {
            // This code can currently generate 3 encryption function variants.

            var m = new MethodDefUser(
                Random.Variable(),
                MethodSig.CreateStatic(module.CorLibTypes.String, module.CorLibTypes.String, module.CorLibTypes.String),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
            );

            EmEncryptAlgorithm em = (EmEncryptAlgorithm)(Random.U32() % 2);
            EmCipherMode       cm = (EmCipherMode)   (Random.U32() % 2);

            // CBC for TripleDES is not implemented. Do we need to maybe call IV.Pick(8).ToArray()
            if (em == EmEncryptAlgorithm.TripleDES) cm = EmCipherMode.ECB; 

            m.Body = new CilBody();

            foreach (var para in m.Parameters)
            {
                para.CreateParamDef();
                var n = Random.Variable();
                para.Name = n;
                para.ParamDef.Name = n;
            }


            var loc0 = new Local(new SZArraySig(module.CorLibTypes.Byte)); // Data
            var loc1 = new Local(new SZArraySig(module.CorLibTypes.Byte)); // Key
            var loc2 = new Local(new SZArraySig(module.CorLibTypes.Byte)); // IV

            m.Body.Variables.Add(loc0);
            m.Body.Variables.Add(loc1);
            m.Body.Variables.Add(loc2);

            m.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Call.ToInstruction(module.Import(typeof(Convert).GetMethod("FromBase64String", new Type[] { typeof(string) }))));
            m.Body.Instructions.Add(OpCodes.Stloc.ToInstruction(loc0));
            // byte[] loc0 = Convert.FromBase64String(arg0);
            m.Body.Instructions.Add(OpCodes.Call.ToInstruction(module.Import(typeof(Encoding).GetMethod("get_UTF8", new Type[] { }))));
            m.Body.Instructions.Add(OpCodes.Ldarg_1.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(Encoding).GetMethod("GetBytes", new Type[] { typeof(string) }))));
            m.Body.Instructions.Add(OpCodes.Stloc.ToInstruction(loc1));
            // byte[] loc1 = Encoding.UTF8.GetBytes(arg1);
            if (em == EmEncryptAlgorithm.AES)
                m.Body.Instructions.Add(OpCodes.Call.ToInstruction(module.Import(typeof(SHA256).GetMethod("Create", new Type[] { }))));
            else
                m.Body.Instructions.Add(OpCodes.Call.ToInstruction(module.Import(typeof(MD5).GetMethod("Create", new Type[] { }))));
            m.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(loc1));
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(HashAlgorithm).GetMethod("ComputeHash", new Type[] { typeof(byte[]) }))));
            m.Body.Instructions.Add(OpCodes.Stloc.ToInstruction(loc1));
            // loc1 = HashFunction.Create().ComputeHash(loc1);

            if (cm == EmCipherMode.CBC)
            {
                m.Body.Instructions.Add(OpCodes.Call.ToInstruction(module.Import(typeof(MD5).GetMethod("Create", new Type[] { }))));
                m.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(loc1));
                m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(HashAlgorithm).GetMethod("ComputeHash", new Type[] { typeof(byte[]) }))));
                m.Body.Instructions.Add(OpCodes.Stloc.ToInstruction(loc2));
                // byte[] loc2 = MD5.Create().ComputeHash(loc1);
            }
            if (em == EmEncryptAlgorithm.AES)
                m.Body.Instructions.Add(OpCodes.Newobj.ToInstruction(module.Import(typeof(AesCryptoServiceProvider).GetConstructor(new Type[] { }))));
            else
                m.Body.Instructions.Add(OpCodes.Newobj.ToInstruction(module.Import(typeof(TripleDESCryptoServiceProvider).GetConstructor(new Type[] { }))));
            // new Algorithm()
            m.Body.Instructions.Add(OpCodes.Dup.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(loc1));
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(SymmetricAlgorithm).GetMethod("set_Key", new Type[] { typeof(byte[]) }))));
            // .Key = loc1;
            m.Body.Instructions.Add(OpCodes.Dup.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Ldc_I4_2.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(SymmetricAlgorithm).GetMethod("set_Padding", new Type[] { typeof(PaddingMode) }))));
            // .Padding = PaddingMode.PKCS7;
            if (cm == EmCipherMode.CBC)
            {
                m.Body.Instructions.Add(OpCodes.Dup.ToInstruction());
                m.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(loc2));
                m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(SymmetricAlgorithm).GetMethod("set_IV", new Type[] { typeof(byte[]) }))));
                // .IV = loc2;
            }
            m.Body.Instructions.Add(OpCodes.Dup.ToInstruction());
            if (cm == EmCipherMode.CBC)
                m.Body.Instructions.Add(OpCodes.Ldc_I4_1.ToInstruction());
            else
                m.Body.Instructions.Add(OpCodes.Ldc_I4_2.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(SymmetricAlgorithm).GetMethod("set_Mode", new Type[] { typeof(CipherMode) }))));
            // .Mode = EmCipherMode;
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(SymmetricAlgorithm).GetMethod("CreateDecryptor", new Type[] { }))));
            // .CreateDecryptor();
            m.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(loc0));
            m.Body.Instructions.Add(OpCodes.Ldc_I4_0.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(loc0));
            m.Body.Instructions.Add(OpCodes.Ldlen.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Conv_I4.ToInstruction());
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(ICryptoTransform).GetMethod("TransformFinalBlock", new Type[] { typeof(byte[]), typeof(int), typeof(int) }))));
            m.Body.Instructions.Add(OpCodes.Stloc.ToInstruction(loc0));
            // loc0 = TransformFinalBlock(loc0, 0, loc0.Length);
            m.Body.Instructions.Add(OpCodes.Call.ToInstruction(module.Import(typeof(Encoding).GetMethod("get_UTF8", new Type[] { }))));
            m.Body.Instructions.Add(OpCodes.Ldloc.ToInstruction(loc0));
            m.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(module.Import(typeof(Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }))));
            m.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            // return Encoding.UTF8.GetBytes(loc0);

            DecryptionMethod decryptionMethod = new DecryptionMethod();
            decryptionMethod.encryptionAlgorithm = em;
            decryptionMethod.cipherMode = cm;
            decryptionMethod.decryptionMethod = m;

            return decryptionMethod;
        }

        public struct DecryptionMethod
        {
            public EmEncryptAlgorithm encryptionAlgorithm;
            public EmCipherMode cipherMode;
            public MethodDefUser decryptionMethod;
        }

        public enum EmEncryptAlgorithm { AES, TripleDES }
        public enum EmCipherMode { ECB, CBC }
    }
}
