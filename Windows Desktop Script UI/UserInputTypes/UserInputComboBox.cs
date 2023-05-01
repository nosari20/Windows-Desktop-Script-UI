using Microsoft.UI.Xaml;
using System;
using Windows_Desktop_Script_UI.InputTypes;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    internal class UserInputComboBox : IUserInput
    {

        private ComboBox m_element;

        private IList ValuesList;

        public UserInputComboBox(string fileUri) :base(fileUri)
        {
            m_element = new ComboBox();
            ValuesList = new List<string>();
            m_element.ItemsSource = ValuesList;
            m_element.SelectedIndex = 0;
            Header = "Choose value";

        }

        public override UIElement GetElement()
        {
            m_element.Header = Header;
            if(AllowedValues != "")
            {
                ValuesList = AllowedValues.Split("|").ToList();
                m_element.ItemsSource = ValuesList;
                m_element.SelectedIndex = 0;
            }

            if(Value != "")
            {
                int index = ValuesList.IndexOf(Value);
                m_element.SelectedIndex = (index >= 0 ? index : 0);
            }

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
            if(m_element.SelectedIndex >= 0 & ValuesList.Count > 0)
            {
                return (string)ValuesList[m_element.SelectedIndex];
            }
            else
            {
                return "";
            }
        }
    }
}
