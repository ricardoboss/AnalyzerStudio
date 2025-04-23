using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using AnalyzerStudio.Models;

namespace AnalyzerStudio;

/// <summary>
/// Interaction logic for PropertyEditorWindow.xaml
/// </summary>
public partial class PropertyEditorWindow : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private void SetProperty<T>(ref T currentValue, T newValue, [CallerMemberName] string? propertyName = null, params string[] propertyNames)
	{
		if (currentValue != null && currentValue.Equals(newValue))
			return;

		currentValue = newValue;

		var properties = new List<string?>(propertyNames) { propertyName };

		foreach (var prop in properties)
			PropertyChanged?.Invoke(this, new(prop));
	}

	private Property property;
	public Property Property
	{
		get => property;
		set => SetProperty(ref property, value);
	}

	private int selectedTypeIndex;
	public int SelectedTypeIndex
	{
		get => selectedTypeIndex;
		set => SetProperty(ref selectedTypeIndex, value);
	}

	private int selectedStrategyIndex;
	public int SelectedStrategyIndex
	{
		get => selectedStrategyIndex;
		set => SetProperty(ref selectedStrategyIndex, value, nameof(SelectedStrategyIndex), nameof(NormalizationStrategyDescription));
	}

	public string NormalizationStrategyDescription
	{
		get
		{
			return (Enum.GetValues<NormalizationStrategy>().GetValue(SelectedStrategyIndex) as NormalizationStrategy?) switch
			{
				NormalizationStrategy.Max => "Maps values from (min,0) to (max,1) linearly",
				NormalizationStrategy.Min => "Maps values from (min,1) to (max,0) linearly",
				NormalizationStrategy.InverseMax => "DO NOT USE - Obsolete",
				NormalizationStrategy.QuartMax => "Maps values from (min,0) to (max,1) quartically (bent towards 0)",
				NormalizationStrategy.InverseQuartMax => "Maps values from (min,0) to (max,1) quartically (bent towards 1)",
				NormalizationStrategy.QuartMin => "Maps values from (min,1) to (max,0) quartically (bent towards 0)",
				NormalizationStrategy.InverseQuartMin => "Maps values from (min,1) to (max,0) quartically (bent towards 1)",
				null => "Invalid selection",
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
	}

	public PropertyEditorWindow(Property property)
	{
		InitializeComponent();

		DataContext = this;

		this.property = property;

		selectedTypeIndex = Array.IndexOf(
			Enum.GetValues<PropertyType>(),
			property.Type
		);

		SelectedStrategyIndex = Array.IndexOf(
			Enum.GetValues<NormalizationStrategy>(),
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

	private void ComboBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Property.Type = (PropertyType)(Enum.GetValues<PropertyType>().GetValue(SelectedTypeIndex) ?? throw new NullReferenceException());
	}

	private void ComboBoxNormalizationStrategy_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Property.NormalizationStrategy = (NormalizationStrategy)(Enum.GetValues<NormalizationStrategy>().GetValue(SelectedStrategyIndex) ?? throw new NullReferenceException());
	}
}
