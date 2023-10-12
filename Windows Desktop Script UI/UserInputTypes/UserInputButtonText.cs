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

        private RichTextBlock m_element;


        public UserInputButtonText(string fileUri) : base(fileUri)
        {
        }

        public override UIElement GetElement()
        {

            string TemplateXAML = string.Format(@"
                        <RichTextBlock
                            xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                                        {0}  
                        </RichTextBlock>
                    ", Value);

            try
            {
                m_element = (RichTextBlock)XamlReader.Load(TemplateXAML);
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
