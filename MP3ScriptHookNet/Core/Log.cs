using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3ScriptHookNet
{
    public static class Log
    {
        private const string fileName = "MP3ScriptHookNet";
        private const string fileExtension = "log";
        private const string dateFormat = "MM-dd-yyyy";

        public static string Filename
        {
            get
            {
                return string.Format("{0}.{1}", fileName, fileExtension);
            }
        }

        /// <summary>
        /// Print a message to the log
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        /// <param name="showInConsole"></param>
        public static void Print(string tag, string message, bool showInConsole = true)
        {
            string output = string.Format("({0}) {1}: {2}", DateTime.Now.ToString("u").Replace("Z", string.Empty), tag, message);
            Console.WriteLine(output);

            using (StreamWriter file = new StreamWriter(Filename, true))
            {
                file.WriteLine(output);
            }

            if(showInConsole)
                Console.WriteLine(output);
        }
    }
}
