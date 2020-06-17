﻿using Dynamitey;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
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
    public partial class SpecimenEditorWindow : Window, INotifyPropertyChanged
    {
        private readonly IEnumerable<Property> Properties;
        private readonly Dictionary<Property, TextBox> propertyInputMap = new Dictionary<Property, TextBox>();

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

                input.LostFocus += Input_LostFocus;
                input.TextChanged += Input_TextChanged;

                row++;
            }
        }

        public bool Edit(ref Specimen source)
        {
            TextBoxName.Text = source.Name;

            foreach (var pair in propertyInputMap)
                pair.Value.DataContext = source;

            UpdateCanSave();
            TextBoxName.Focus();

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
            UpdateCanSave();
        }

        private void UpdateCanSave(bool validateText = false)
        {
            if (validateText)
                CanSave = propertyInputMap.Values.All(tb => double.TryParse(tb.Text, out _));
            else
                CanSave = !propertyInputMap.Values.Any(tb => Validation.GetHasError(tb));
        }
    }
}