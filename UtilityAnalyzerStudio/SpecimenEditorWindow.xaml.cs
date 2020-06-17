using Dynamitey;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using UtilityAnalyzerStudio.Converters;
using UtilityAnalyzerStudio.Models;
using UtilityAnalyzerStudio.Rules;

namespace UtilityAnalyzerStudio
{
    /// <summary>
    /// Interaction logic for SpecimenEditorWindow.xaml
    /// </summary>
    public partial class SpecimenEditorWindow : Window, INotifyPropertyChanged
    {
        private readonly IEnumerable<Property> Properties;
        private readonly Dictionary<Property, FrameworkElement> propertyInputMap = new Dictionary<Property, FrameworkElement>();

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanSave
        {
            get => canSave;
            set
            {
                canSave = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSave)));
            }
        }
        private bool canSave = false;


        public SpecimenEditorWindow(IEnumerable<Property> properties)
        {
            InitializeComponent();

            DataContext = this;
            Properties = properties;

            int row = 2; // row = 0 are headers, row = 1 is the name
            foreach (var prop in properties)
            {
                var propRow = GenerateRowDefinition();
                GridProperties.RowDefinitions.Add(propRow);

                var label = GenerateLabel(row, prop.Name);
                GridProperties.Children.Add(label);

                var input = GenerateInput(row, prop);
                GridProperties.Children.Add(input);

                propertyInputMap[prop] = input;

                input.LostFocus += Input_LostFocus;

                row++;
            }
        }

        private RowDefinition GenerateRowDefinition()
        {
            return new RowDefinition
            {
                Height = new GridLength(0, GridUnitType.Auto)
            };
        }

        private TextBlock GenerateLabel(int row, string text)
        {
            var label = new TextBlock
            {
                Text = text
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);

            return label;
        }

        private FrameworkElement GenerateInput(int row, Property property)
        {
            ValidationRule validationRule = property.Type switch
            {
                PropertyType.Double => new ParseableDoubleRule(),
                _ => null
            };

            FrameworkElement input;
            switch (property.Type)
            {
                case PropertyType.Double:
                    input = new TextBox { Text = "0" };
                    ((TextBox)input).TextChanged += Input_TextChanged;
                    break;
                case PropertyType.Text:
                    input = new TextBox();
                    ((TextBox)input).TextChanged += Input_TextChanged;
                    break;
                case PropertyType.Boolean:
                    input = new CheckBox { IsChecked = false };
                    ((CheckBox)input).Click += Input_IsCheckedChanged;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var binding = new Binding
            {
                Path = new PropertyPath(property.Name)
            };

            if (validationRule != null)
                binding.ValidationRules.Add(validationRule);

            if (property.Type.Equals(PropertyType.Double))
                binding.Converter = new TextToDoubleConverter();

            DependencyProperty bindingProperty = property.Type switch
            {
                PropertyType.Double => TextBox.TextProperty,
                PropertyType.Text => TextBox.TextProperty,
                PropertyType.Boolean => CheckBox.IsCheckedProperty,
                _ => throw new NotImplementedException()
            };

            BindingOperations.SetBinding(input, bindingProperty, binding);

            Grid.SetRow(input, row);
            Grid.SetColumn(input, 1);

            return input;
        }

        public bool Edit(ref Specimen source)
        {
            DataContext = source;

            UpdateCanSave();
            TextBoxName.Focus();

            var result = ShowDialog();
            return result.HasValue && result.Value;
        }

        private void PopulateFromEditorValues(Specimen specimen)
        {
            specimen.Name = TextBoxName.Text;

            foreach (var prop in Properties)
            {
                var inputField = propertyInputMap[prop];
                if (!TryGetValue(inputField, prop.Type, out var value))
                    continue;

                Dynamic.InvokeSet(specimen, prop.Name, value);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ButtonSave.Focus();

            UpdateCanSave(true);
            if (!CanSave)
                return;

            DialogResult = true;

            Close();
        }

        private void ButtonAbort_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }

        private void Input_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateCanSave();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCanSave(true);
        }

        private void Input_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateCanSave();
        }

        private void UpdateCanSave(bool validateText = false)
        {
            if (validateText)
                CanSave = propertyInputMap.All(pair => TryGetValue(pair.Value, pair.Key.Type, out _));
            else
                CanSave = !propertyInputMap.Values.Any(input => Validation.GetHasError(input));
        }

        private bool TryGetValue(FrameworkElement input, PropertyType type, out object value)
        {
            value = null;

            if (Validation.GetHasError(input))
                return false;

            switch (type)
            {
                case PropertyType.Double:
                    value = 0d;

                    if (!(input is TextBox tb))
                        return false;

                    if (!double.TryParse(tb.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var doub))
                        return false;

                    value = doub;

                    return true;
                case PropertyType.Text:
                    value = "";

                    if (!(input is TextBox tb2))
                        return false;

                    value = tb2.Text;

                    return true;
                case PropertyType.Boolean:
                    value = false;

                    if (!(input is CheckBox cb))
                        return false;

                    value = cb.IsChecked;

                    return true;
                default:
                    value = null;

                    return false;
            }
        }
    }
}
