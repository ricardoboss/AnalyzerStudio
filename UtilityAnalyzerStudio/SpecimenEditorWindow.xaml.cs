using Dynamitey;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using UtilityAnalyzerStudio.Models;
using UtilityAnalyzerStudio.Rules;

namespace UtilityAnalyzerStudio
{
    /// <summary>
    /// Interaction logic for SpecimenEditorWindow.xaml
    /// </summary>
    public partial class SpecimenEditorWindow : Window
    {
        private readonly IEnumerable<Property> Properties;
        private readonly Dictionary<Property, TextBox> propertyInputMap = new Dictionary<Property, TextBox>();

        public SpecimenEditorWindow(IEnumerable<Property> properties)
        {
            InitializeComponent();

            DataContext = this;
            Properties = properties;

            var validationRule = new ParseableDoubleRule();

            int row = 2; // row = 0 are headers, row = 1 is the name
            foreach (var prop in properties)
            {
                var propRow = new RowDefinition
                {
                    Height = new GridLength(0, GridUnitType.Auto)
                };
                GridProperties.RowDefinitions.Add(propRow);

                var label = new TextBlock
                {
                    Text = prop.Name
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                GridProperties.Children.Add(label);

                var input = new TextBox();
                var binding = new Binding
                {
                    Path = new PropertyPath(prop.Name)
                };
                binding.ValidationRules.Add(validationRule);
                BindingOperations.SetBinding(input, TextBox.TextProperty, binding);

                Grid.SetRow(input, row);
                Grid.SetColumn(input, 1);
                GridProperties.Children.Add(input);

                propertyInputMap[prop] = input;

                row++;
            }
        }

        public bool Edit(ref Specimen source)
        {
            TextBoxName.Text = source.Name;

            foreach (var pair in propertyInputMap)
            {
                //var value = Dynamic.InvokeGet(source, pair.Key.Name);
                pair.Value.DataContext = source;
            }

            var result = ShowDialog();
            if (!result.HasValue || !result.Value)
                return false;

            PopulateFromEditorValues(source);

            return true;
        }

        private void PopulateFromEditorValues(Specimen specimen)
        {
            specimen.Name = TextBoxName.Text;

            foreach (var prop in Properties)
            {
                var inputField = propertyInputMap[prop];
                var value = inputField.Text;

                Dynamic.InvokeSet(specimen, prop.Name, value);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Close();
        }

        private void ButtonAbort_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}
