using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace Windows_Desktop_Script_UI.InputTypes
{
    internal abstract class IUserInput
    {

        // Customizeable fields
        public string PlaceHolder = "";
        public string Header = "Enter value";
        public string AllowedValues = "";
        public string Value = "";
        public double Height = 0, Width = 0;

        //File output path
        private string m_OutFileUri;

        public IUserInput(string fileUri)
        {
            m_OutFileUri = fileUri;
        }

        // Get UI object
        public abstract UIElement GetElement();

        // Get input value
        protected abstract string GetValue();

        // Validate value
        public abstract bool Validate();

        // ToString
        public override string ToString()
        {
            return "[Type: " + (GetElement() != null ? GetElement().GetType().Name : null) + ", PlaceHolder: " + PlaceHolder + ", Header: " + Header + ", AllowedValues: " + AllowedValues + ", Value: " + Value + ", GetValue(): " + GetValue() + ", m_OutFileUri: " + m_OutFileUri + ", Height: " + Height + ", Width: " + Width + "]";
        }

        // Write output to file
        public void WriteOutput()
        {
            try
            {
                using (StreamWriter writetext = new StreamWriter(m_OutFileUri))
                {
                    writetext.WriteLine(GetValue());
                }
                Log.Write("Write to '" + m_OutFileUri + "' succesfull");
            }
            catch(Exception ex)
            {
                Log.Write("Cannot write to '" + m_OutFileUri + "'\b" + ex);
            }
            
        }
    }
}
