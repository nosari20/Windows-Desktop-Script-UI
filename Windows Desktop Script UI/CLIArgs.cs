using System.Collections.Generic;

namespace Windows_Desktop_Script_UI
{
    internal class CLIArgs
    {

        const string PREFIX_FLAG = "-";
        const string PREFIX_OPTION = "--";

        IDictionary<string, string> m_options = new Dictionary<string, string>();
        IList<string> m_flags = new List<string>();

        public CLIArgs(string[] cmdargs) 
        {

            string[] strTmp;
            for (int i = 1; i < cmdargs.Length; i++)
            {

                if(cmdargs[i].StartsWith(PREFIX_OPTION))
                {
                    strTmp = cmdargs[i].Replace(PREFIX_OPTION, "").Split("=");
                    m_options.Add(strTmp[0], strTmp[1]);

                }
                else if (cmdargs[i].StartsWith(PREFIX_FLAG)){
                    m_flags.Add(cmdargs[i].Replace(PREFIX_FLAG, ""));
                }
            }
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

            string output = "";

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
