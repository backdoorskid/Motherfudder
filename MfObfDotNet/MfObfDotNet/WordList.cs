using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public partial class Random
    {
        public static string[] WordListWords = Properties.Resources.WordList.Split('\n');
        public static List<string> VariableCheckList = new List<string>();

        public static string Variable()
        {
GenerateVariable:
            string result = "";
            for (var i = 0; i < 3; i++)
            {
                result += WordListWords[U32() % WordListWords.Length];
            }
            if (VariableCheckList.Contains(result))
            {
                goto GenerateVariable;
            }
            return result;
        }
    }
}
