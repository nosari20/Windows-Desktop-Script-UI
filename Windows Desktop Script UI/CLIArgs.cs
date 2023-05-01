using System.Collections.Generic;
using System.Linq;

namespace Windows_Desktop_Script_UI
{
    internal class CLIArgs
    {

        private const string PREFIX_FLAG = "-";
        private const string PREFIX_OPTION = "--";


        private string command = "";
        private IDictionary<string, string> m_options = new Dictionary<string, string>();
        private IList<string> m_flags = new List<string>();

        public CLIArgs(string[] cmdargs) 
        {

            string[] strTmp;
            if (cmdargs.Count() == 0)
            {
                return;
            }

            command = cmdargs[0].TrimStart('"').TrimEnd('"').TrimStart('\'').TrimEnd('\'');
            for (int i = 1; i < cmdargs.Length; i++)
            {

                if(cmdargs[i].StartsWith(PREFIX_OPTION))
                {
                    strTmp = cmdargs[i].Replace(PREFIX_OPTION, "").Split(new[] { '=' }, 2);
                    m_options.Add(strTmp[0], strTmp[1].TrimStart('"').TrimEnd('"').TrimStart('\'').TrimEnd('\''));

                }
                else if (cmdargs[i].StartsWith(PREFIX_FLAG)){
                    m_flags.Add(cmdargs[i].Replace(PREFIX_FLAG, ""));
                }
            }
        }

        // return command (i.e. first item)
        public string getCommand()
        {
            return command; 
        }


        // return true if option is used
        public bool hasOption(string option)
        {
            return m_options.ContainsKey(option);
        }


        // return true if flag is used
        public bool hasFlag(string flag)
        {
            return m_flags.Contains(flag);
        }

        // return option value
        public string getOption(string option) 
        {
            return m_options[option];
        }

        public string showArgs()
        {

            string output = "Command: " + command + "\n";

            output += "Flags:\n";
            foreach (string arg in m_flags) { 
                output += arg + "\n";
            }
            output += "Options:\n";
            foreach (string arg in m_options.Keys)
            {
                output += arg + "=" + m_options[arg] + "\n";
            }
            return output;
         }



    }
}
