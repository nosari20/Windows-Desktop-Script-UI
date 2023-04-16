using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Desktop_Script_UI
{
    internal class Log
    {

        public static void Write(string str)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff") + "]    " + str);
        }
    }
}
