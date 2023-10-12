using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Media.Core;
using Windows_Desktop_Script_UI.InputTypes;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    internal class UserInputButtonVideo : IUserInput
    {

        private MediaPlayerElement m_element;

        public bool AreTransportControlsEnabled = false;
        public bool AutoPlay = false;
        public bool IsMuted = true;



        public UserInputButtonVideo(string fileUri) : base(fileUri)
        {
             
        }

        public override UIElement GetElement()
        {

            m_element = new MediaPlayerElement();
            m_element.Source = MediaSource.CreateFromUri(new Uri(Value, UriKind.Relative));
            m_element.AreTransportControlsEnabled = AreTransportControlsEnabled;
            m_element.MediaPlayer.IsMuted = IsMuted;
            m_element.AutoPlay = AutoPlay;
            if (Height > 0) m_element.MaxHeight = Height;
            if (Width > 0) m_element.MaxWidth = Width;

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
