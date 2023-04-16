using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Windows_Desktop_Script_UI
{


    public class FileWatcher
    {

        // File path to watch
        private string m_filePath;

        // FileSystemWatcher instance 
        private FileSystemWatcher m_watcher;

        // Debug mode
        private bool m_debug;

        // Lines already read
        private IList<string> m_Lines = new List<string>();

        // Event for new line
        public event EventHandler<IList<string>> NewLine;

        


        public FileWatcher(string filePath, bool debug=false)
        {
            m_filePath = filePath;

            m_debug = debug;

            Log.Write("Creating watcher: path=" + Path.GetDirectoryName(m_filePath) + " file=" + Path.GetFileName(m_filePath));

            // Creating watcher
            m_watcher = new FileSystemWatcher(Path.GetDirectoryName(m_filePath));

            // Watch for write events
            m_watcher.NotifyFilter = NotifyFilters.LastWrite;

            // Settup handlers
            m_watcher.Changed += OnChanged;
            m_watcher.Error += OnError;

            // Filter to filename
            m_watcher.Filter = Path.GetFileName(m_filePath);

            // Allow events
            m_watcher.EnableRaisingEvents = true;

        }


        // File update handler
        private void OnChanged(object sender, FileSystemEventArgs e)
        {

            // Return if not of changed type
            if (e.ChangeType != WatcherChangeTypes.Changed) return;

            // Check if file exist
            if (File.Exists(m_filePath))
            {

                // Read content
                string line;
                int nbLines = m_Lines.Count;
                List<string> newLines = new List<string>();

                using (FileStream fs = new FileStream(m_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs))
                {
                    int index = 0;
                    while (sr.Peek() >= 0)
                    {
                        line = sr.ReadLine();
                        
                        // Ignore empty lines
                        if(line != "")
                        {
                            // Ignore read lines
                            if (index >= nbLines)
                            {
                                m_Lines.Add(line);
                                
                                // Add to new lines to pass to handlers
                                newLines.Add(line);
                            }
                            index++;
                        }     
                    }
                }

                // Do not trigger change if nothing is read
                if (newLines.Count > 0)
                {
                    // Invoke handler
                    NewLine.Invoke(this, newLines);
                }

            }
        }


        // Handle Errors
        private void OnError(object sender, ErrorEventArgs e)
        {
            PrintException(e.GetException());
        }
            

        private void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Log.Write("Message: {ex.Message}");
                Log.Write("Stacktrace:");
                Log.Write(ex.StackTrace);
                PrintException(ex.InnerException);
            }
        }

    }
}
