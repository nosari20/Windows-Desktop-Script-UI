using Microsoft.UI.Xaml;
using System;
using Windows_Desktop_Script_UI.InputTypes;
using Microsoft.UI.Xaml.Controls;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    internal class UserInputPassword : IUserInput
    {

        private PasswordBox m_element;

        public UserInputPassword(string fileUri) :base(fileUri)
        {
            m_element = new PasswordBox();
            Header = "Enter password";
            
        }

        public override UIElement GetElement()
        {
            m_element.Header = Header;
            m_element.PlaceholderText = PlaceHolder;
            if (Height > 0) m_element.Height = Height;
            if (Width > 0) m_element.Width = Width;
            return m_element;
        }

        public override bool Validate()
        {
            return true;
        }

        protected override string GetValue()
        {
            return m_element.Password;
        }
    }
}
