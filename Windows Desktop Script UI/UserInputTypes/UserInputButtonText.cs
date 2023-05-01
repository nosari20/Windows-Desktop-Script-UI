using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Media.Core;
using Windows_Desktop_Script_UI.InputTypes;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    internal class UserInputButtonText : IUserInput
    {

        private ScrollViewer m_element;


        public UserInputButtonText(string fileUri) : base(fileUri)
        {
            Height = 250;
        }

        public override UIElement GetElement()
        {

            string TemplateXAML = string.Format(@"
                        <ScrollViewer
                            xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                            Height='{1}'>
                                <RichTextBlock>
                                        {0}  
                                </RichTextBlock>
                        </ScrollViewer>
                    ", Value,Height);

            try
            {
                m_element = (ScrollViewer)XamlReader.Load(TemplateXAML);
                if (Height > 0) m_element.Height = Height;
                if (Width > 0) m_element.Width = Width;
            }
            catch (Microsoft.UI.Xaml.Markup.XamlParseException e)
            {
                Log.Write(e.ToString());
            }

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
