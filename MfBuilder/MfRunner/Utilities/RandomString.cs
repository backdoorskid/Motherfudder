using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        static Random RandomGenerator = new Random();
        public static string GenerateRandomString()
        {
            int size = RandomGenerator.Next(10, 20);
            char[] stringCharacters = new char[size];
            string charactersList   = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            for (int i = 0; i < size; i++) {
                stringCharacters[i] = charactersList[RandomGenerator.Next(62)]; 
            }

            return new string(stringCharacters);
        }
    }
}
