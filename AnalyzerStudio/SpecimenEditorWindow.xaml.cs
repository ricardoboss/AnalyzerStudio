using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

using AnalyzerStudio.Converters;
using AnalyzerStudio.Models;
using AnalyzerStudio.Rules;

namespace AnalyzerStudio;

/// <summary>
/// Interaction logic for SpecimenEditorWindow.xaml
/// </summary>
public partial class SpecimenEditorWindow : INotifyPropertyChanged
{
	private readonly Dictionary<Property, FrameworkElement> propertyInputMap = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	private bool canSave;
	public bool CanSave
	{
		get => canSave;
		set
		{
			canSave = value;

			PropertyChanged?.Invoke(this, new(nameof(CanSave)));
		}
	}

	public SpecimenEditorWindow(IEnumerable<Property> properties)
	{
		InitializeComponent();

		DataContext = this;

		var row = 3; // row = 0 are headers, row = 1 is the separator, row = 2 is the name
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

	private static RowDefinition GenerateRowDefinition()
	{
		return new()
		{
			Height = new(0, GridUnitType.Auto),
		};
	}

	private static TextBlock GenerateLabel(int row, string text)
	{
		var label = new TextBlock
		{
			Text = text,
		};

		Grid.SetRow(label, row);
		Grid.SetColumn(label, 0);

		return label;
	}

	private FrameworkElement GenerateInput(int row, Property property)
	{
		var validationRule = property.Type switch
		{
			PropertyType.Double => new ParseableDoubleRule(),
			_ => null,
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
			Path = new(property.Name)
		};

		if (validationRule != null)
			binding.ValidationRules.Add(validationRule);

		if (property.Type.Equals(PropertyType.Double))
			binding.Converter = new TextToDoubleConverter();

		var bindingProperty = property.Type switch
		{
			PropertyType.Double => TextBox.TextProperty,
			PropertyType.Text => TextBox.TextProperty,
			PropertyType.Boolean => ToggleButton.IsCheckedProperty,
			_ => throw new NotImplementedException(),
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

	private void ButtonSave_Click(object sender, RoutedEventArgs e)
	{
		ButtonSave.Focus();

		UpdateCanSave(true);
		if (!CanSave)
			return;

		DialogResult = true;
	}

	private void ButtonAbort_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = false;
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
			CanSave = !propertyInputMap.Values.Any(Validation.GetHasError);
	}

	private static bool TryGetValue(FrameworkElement input, PropertyType type, out object? value)
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
