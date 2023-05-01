using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows_Desktop_Script_UI.InputTypes;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    internal class UserInputButton : IUserInput
    {


        public UserInputButton(string fileUri) : base(fileUri)
        {
        }

        public override UIElement GetElement()
        {
            return null;
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
