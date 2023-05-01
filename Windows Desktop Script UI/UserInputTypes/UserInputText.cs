using Microsoft.UI.Xaml;
using Windows_Desktop_Script_UI.InputTypes;
using Microsoft.UI.Xaml.Controls;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    internal class UserInputText : IUserInput
    {

        private TextBox m_element;

        public UserInputText(string fileUri) :base(fileUri)
        {
            m_element = new TextBox();
            
        }

        public override UIElement GetElement()
        {
            m_element.Text = Value;
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
            return m_element.Text;
        }
    }
}
