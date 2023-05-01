using Microsoft.UI.Xaml;
using System;
using Windows_Desktop_Script_UI.InputTypes;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Markup;
using System.Collections.ObjectModel;

namespace Windows_Desktop_Script_UI.UserInputTypes
{
    class UserInputImageChooser : IUserInput
    {

        public class InputImage
        {
            public string Title { get; set; } = "";
            public string ImageLocation { get; set; }
        }
        
        private GridView m_element;
        private StackPanel m_panel;
        private InputImage m_selected;

        private IList<InputImage> ValuesList;

        public UserInputImageChooser(string fileUri) :base(fileUri)
        {

            ValuesList = new ObservableCollection<InputImage>();
            Width = 200;

        }

        public override UIElement GetElement()
        {

            m_panel = new StackPanel();
            m_panel.Orientation = Orientation.Vertical;

            m_element = new GridView();
            m_element.SelectionMode = ListViewSelectionMode.Single;
            m_element.IsItemClickEnabled = true;

            m_element.ItemClick += (sender, e) =>
            {
                m_selected = (InputImage)e.ClickedItem;
            };

            m_panel.Children.Add(new TextBlock()
            {
                Text = Header,
                Margin = new Thickness()
                {
                    Bottom = 10
                }
            });
            m_panel.Children.Add(m_element);



            string TemplateXAML = @"
                <DataTemplate 
                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                    <StackPanel
                        Orientation='Vertical'
                        HorizontalAlignment='Center'
                        VerticalAlignment='Center'>
                        <Image 
                            Stretch='UniformToFill' 
                            Source='{Binding ImageLocation}' 
                            Width='" + Width.ToString()+@"' 
                            "+ (Height > 0 ? "Height='" + Height.ToString() + "'" : "") + @"
                            Margin='0,0,5,8'
                        />
                        <TextBlock Text='{Binding Title}' Margin='0,0,0,10' HorizontalAlignment=""Center""/>
                    </StackPanel>
                </DataTemplate>
            ";


            DataTemplate controlTemplate = (DataTemplate)XamlReader.Load(TemplateXAML);
            m_element.ItemTemplate = controlTemplate;



            if (AllowedValues != "")
            {
                ValuesList.Clear();
                foreach (string item in AllowedValues.Split("|").ToList())
                {
                    string[] info = item.Split("#");
                    InputImage image = new InputImage();
                    image.Title = (info.Length > 1 ? info[1] : "");
                    image.ImageLocation = info[0];
                    ValuesList.Add(image);
                }

                m_element.ItemsSource = ValuesList;
            }

            return m_panel;
        }

        public override bool Validate()
        {
            return m_element.SelectedItem != null;
        }

        protected override string GetValue()
        {
            if(m_selected != null)
            {
                return m_selected.ImageLocation;
            }
            else
            {
                return "";
            }
        }

    }
}
