using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Media.Core;
using Windows_Desktop_Script_UI.InputTypes;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    internal class UserInputButtonImage : IUserInput
    {

        private Image m_element;


        public UserInputButtonImage(string fileUri) : base(fileUri)
        {
             
        }

        public override UIElement GetElement()
        {
          
            m_element = new Image();
            m_element.Source = new BitmapImage(new Uri(Value, UriKind.Relative));
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
            return "";
        }
    }
}
