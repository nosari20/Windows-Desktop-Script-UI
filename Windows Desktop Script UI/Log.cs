using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Desktop_Script_UI
{
    internal class Log
    {

        public enum Level
        { 
            Debug, 
            Normal
        }

        public static Level level = Level.Normal;

        public static void Write(string str)
        {
            if(level == Level.Debug)
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff") + "]    " + str);
        }
    }
}
