﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Linq;
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
    /// Interaction logic for PropertyEditorWindow.xaml
    /// </summary>
    public partial class PropertyEditorWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null, params string[] propertyNames)
        {
            property = value;

            var properties = new List<string>(propertyNames) { propertyName };

            foreach (var prop in properties)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Property property;
        public Property Property
        {
            get => property;
            set => SetProperty(ref property, value);
        }

        private int selectedStrategyIndex;
        public int SelectedStrategyIndex
        {
            get => selectedStrategyIndex;
            set => SetProperty(ref selectedStrategyIndex, value);
        }

        public PropertyEditorWindow(Property property)
        {
            InitializeComponent();

            DataContext = this;

            Property = property;

            SelectedStrategyIndex = Array.IndexOf(
                Enum.GetValues(typeof(NormalizationStrategy)),
                property.NormalizationStrategy
            );

            TextBoxName.Focus();
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

        private void ComboBoxNormalizationStrategy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Property.NormalizationStrategy = (NormalizationStrategy) Enum.GetValues(typeof(NormalizationStrategy)).GetValue(SelectedStrategyIndex);
        }
    }
}
