using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace UtilityAnalyzerStudio
{
    /// <summary>
    /// Interaktionslogik für PromptWindow.xaml
    /// </summary>
    public partial class PromptWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null, params string[] propertyNames)
        {
            property = value;

            var properties = new List<string>(propertyNames) { propertyName };

            foreach (var prop in properties)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value, nameof(Description), nameof(DescriptionVisibility));
        }
        private string description;

        public Visibility DescriptionVisibility => Description != null && Description.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

        private Dictionary<string, TextBox> InputMapping { get; } = new Dictionary<string, TextBox>();
        public ImmutableDictionary<string, string> InputValues => InputMapping.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.Text)).ToImmutableDictionary();

        public PromptWindow(string key, string display) : this(new KeyValuePair<string, string>(key, display))
        {
        }

        public PromptWindow(params KeyValuePair<string, string>[] inputs)
        {
            InitializeComponent();
            InitializeInputs(inputs);

            DataContext = this;
        }

        private void InitializeInputs(IEnumerable<KeyValuePair<string, string>> inputs)
        {
            var row = 0;
            foreach (var pair in inputs)
            {
                var rowDef = new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Auto)
                };
                GridInputs.RowDefinitions.Add(rowDef);

                var label = new TextBlock
                {
                    Text = pair.Value
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);

                var input = new TextBox();
                Grid.SetRow(input, row);
                Grid.SetColumn(input, 1);

                GridInputs.Children.Add(label);
                GridInputs.Children.Add(input);

                InputMapping[pair.Key] = input;

                row++;
            }

            if (row > 0)
                InputMapping.First().Value.Focus();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
