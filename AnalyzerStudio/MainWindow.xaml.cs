using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using AnalyzerStudio.Extensions;
using AnalyzerStudio.Models;

namespace AnalyzerStudio;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
	#region Properties

	public string MainTitle => $"{CurrentProject.ProjectTitle} - {App.Title}";

	public event PropertyChangedEventHandler? PropertyChanged;

	private AnalysisProject? currentProject;
	public AnalysisProject CurrentProject
	{
		get => currentProject ?? throw new NullReferenceException();
		private set
		{
			currentProject = value;

			PropertyChanged?.Invoke(this, new(nameof(CurrentProject)));
			PropertyChanged?.Invoke(this, new(nameof(MainTitle)));

			currentProject.PropertyChanged += (o, e) =>
			{
				if (e.PropertyName.Equals(nameof(AnalysisProject.ProjectTitle)))
					PropertyChanged?.Invoke(this, new(nameof(MainTitle)));
			};
		}
	}

	private Specimen? selectedSpecimen;
	public Specimen? SelectedSpecimen
	{
		get => selectedSpecimen;
		set
		{
			selectedSpecimen = value;

			PropertyChanged?.Invoke(this, new(nameof(SelectedSpecimen)));
		}
	}

	private Property? selectedProperty;
	public Property? SelectedProperty
	{
		get => selectedProperty;
		set
		{
			selectedProperty = value;

			PropertyChanged?.Invoke(this, new(nameof(SelectedProperty)));
		}
	}

	private AnalysisDataset? selectedDataset;
	public AnalysisDataset? SelectedDataset
	{
		get => selectedDataset;
		set
		{
			selectedDataset = value;

			PropertyChanged?.Invoke(this, new(nameof(SelectedDataset)));
		}
	}

	#endregion

	public MainWindow(AnalysisProject project)
	{
		InitializeComponent();

		DataContext = this;

		Closing += MainWindow_Closing;

		LoadProject(project);
	}

	#region Project Management

	private void MainWindow_Closing(object sender, CancelEventArgs e)
	{
		if (!CurrentProject.IsDirty)
			return;

		var result = MessageBox.Show("Your project has unsaved changes! Do you want to save them?", "Unsaved Project Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
		if (!result.Equals(MessageBoxResult.Yes))
		{
			e.Cancel = result.Equals(MessageBoxResult.Cancel);

			return;
		}

		CurrentProject.Save();
	}

	private void LoadProject(AnalysisProject project)
	{
		if (currentProject != null)
		{
			currentProject.Properties.CollectionChanged -= Properties_CollectionChanged;

			UpdateSpecimensColumns(currentProject.Properties, new List<Property>());
		}

		project.Properties.CollectionChanged += Properties_CollectionChanged;

		UpdateSpecimensColumns(new List<Property>(), project.Properties);

		project.UpdateAnalysis();

		CurrentProject = project;
	}

	private void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		UpdateSpecimensColumns((e.OldItems ?? new List<Property>()).Cast<Property>(), (e.NewItems ?? new List<Property>()).Cast<Property>());
	}

	public void UpdateSpecimensColumns(IEnumerable<Property> removed, IEnumerable<Property> added)
	{
		foreach (var property in removed)
		{
			var col = SpecimensGridView.Columns.FirstOrDefault(c => c.Header.Equals(property));
			if (col == null)
				continue;

			SpecimensGridView.Columns.Remove(col);
		}

		foreach (var property in added)
		{
			var headerBinding = new Binding
			{
				Path = new(nameof(Property.Name))
			};
			var headerTemplate = new DataTemplate();
			var headerTextBlock = new FrameworkElementFactory(typeof(TextBlock));
			headerTextBlock.SetBinding(TextBlock.TextProperty, headerBinding);
			headerTemplate.VisualTree = headerTextBlock;

			var cellBinding = new Binding
			{
				Path = new(property.Name)
			};
			var cellTemplate = new DataTemplate();
			var cellTextBlock = new FrameworkElementFactory(typeof(TextBlock));
			cellTextBlock.SetBinding(TextBlock.TextProperty, cellBinding);
			cellTemplate.VisualTree = cellTextBlock;

			var col = new GridViewColumn
			{
				Header = property,
				HeaderTemplate = headerTemplate,
				CellTemplate = cellTemplate,
				Width = double.NaN
			};

			GridViewSort.SetPropertyName(col, property.Name);

			SpecimensGridView.Columns.Add(col);
		}
	}

	#endregion

	#region Edit & Create

	private void CreateProperty()
	{
		var property = new Property();
		if (Edit(property))
			CurrentProject.Properties.Add(property);
	}

	private void CreateSpecimen()
	{
		var specimen = new Specimen(CurrentProject.Properties);
		if (Edit(specimen))
			CurrentProject.Specimens.Add(specimen);
	}

	private void ListViewSpecimensItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (!(sender is ListViewItem lvi) || !(lvi.Content is Specimen original))
			return;

		Edit(original);
	}

	private void ListViewPropertiesItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (!(sender is ListViewItem lvi) || !(lvi.Content is Property original))
			return;

		Edit(original);
	}

	private bool Edit(Specimen specimen)
	{
		var editorWindow = new SpecimenEditorWindow(CurrentProject.Properties)
		{
			Owner = this
		};

		var clone = specimen.Clone();
		if (!editorWindow.Edit(ref clone))
			return false;

		specimen.CopyValues(clone);

		return true;
	}

	private bool Edit(Property property)
	{
		var clone = property.Clone();

		var editorWindow = new PropertyEditorWindow(clone)
		{
			Owner = this
		};

		var result = editorWindow.ShowDialog();

		if (!result.HasValue || !result.Value)
			return false;

		property.CopyValues(clone);

		return true;
	}

	#endregion

	#region Commands

	public static RoutedCommand AppExitCommand = new();
	public static RoutedCommand AppAboutCommand = new();
	public static RoutedCommand ProjectChangeNameCommand = new();
	public static RoutedCommand SpecimenCreateCommand = new();
	public static RoutedCommand SpecimenEditCommand = new();
	public static RoutedCommand PropertyCreateCommand = new();
	public static RoutedCommand PropertyEditCommand = new();

	private void CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void ProjectSaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = CurrentProject.IsDirty;
	}

	private void ProjectSaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CurrentProject.Save();
	}

	private void ProjectSaveAtCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		var path = "";
		if (!ProjectLoadWindow.TryGetNewProjectPath(this, CurrentProject.Name, ref path))
			return;

		CurrentProject.Save(path);

		MessageBox.Show("Successfully saved project at " + path, "Project Saved");
	}

	private void ProjectCloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		App.Current.TryCloseCurrent();
	}

	private void ProjectChangeNameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		var name = "";
		if (ProjectLoadWindow.TryGetProjectName(this, ref name, true))
			CurrentProject.Name = name;
	}

	private void AppExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Close();
	}

	private void AppAboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		var aboutWindow = new AboutWindow
		{
			Owner = this
		};
		aboutWindow.Show();
	}

	private void SpecimenCreateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CreateSpecimen();
	}

	private void PropertyCreateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CreateProperty();
	}

	private void SpecimenEditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = SelectedSpecimen != null;
	}

	private void SpecimenEditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (SelectedSpecimen == null)
			return;

		Edit(SelectedSpecimen);
	}

	private void PropertyEditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = SelectedProperty != null;
	}

	private void PropertyEditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (SelectedProperty == null)
			return;

		Edit(SelectedProperty);
	}

	private void DeleteSelectedCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = false;

		var focussed = FocusManager.GetFocusedElement(this);
		if (focussed is null ||
		    !(focussed is ListViewItem lvi) ||
		    lvi.DataContext == null)
			return;

		if (lvi.DataContext is Property || lvi.DataContext is Specimen)
			e.CanExecute = true;
	}

	private void DeleteSelectedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		var focussed = FocusManager.GetFocusedElement(this);
		var context = (focussed as ListViewItem)!.DataContext;

		if (context is Specimen s)
		{
			var result = MessageBox.Show($"Are you sure you want to delete the specimen '{s.Name}'?", "Deleting Specimen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (!result.Equals(MessageBoxResult.Yes))
				return;

			CurrentProject.Specimens.Remove(s);
		}
		else if (context is Property p)
		{
			var result = MessageBox.Show($"Are you sure you want to delete the property '{p.Name}'? This will also remove it from every specimen.", "Deleting Property", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (!result.Equals(MessageBoxResult.Yes))
				return;

			CurrentProject.Properties.Remove(p);
		}
	}

	#endregion

	#region File Extension Registration

	private void MenuItemUninstallExtension_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			RegistryManager.UninstallExtension();

			MessageBox.Show("File association uninstalled!", "File Type Association", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		catch (UnauthorizedAccessException)
		{
			var psi = new ProcessStartInfo(RegistryManager.ExeLocation, "--uninstall-extension");
			if (InvokeAsAdmin(psi))
				MessageBox.Show("File association uninstalled!", "File Type Association", MessageBoxButton.OK, MessageBoxImage.Information);
			else
				MessageBox.Show("Could not uninstall file type!", "File Type Association", MessageBoxButton.OK, MessageBoxImage.Warning);
		}
		catch (ArgumentException)
		{
			MessageBox.Show("File association was already uninstalled!", "File Type Association", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}

	private void MenuItemInstallExtension_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			RegistryManager.InstallExtension();

			MessageBox.Show("File association installed!", "File Type Association", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		catch (UnauthorizedAccessException)
		{
			var psi = new ProcessStartInfo(RegistryManager.ExeLocation, "--install-extension");
			if (InvokeAsAdmin(psi))
				MessageBox.Show("File association installed!", "File Type Association", MessageBoxButton.OK, MessageBoxImage.Information);
			else
				MessageBox.Show("Could not install file type!", "File Type Association", MessageBoxButton.OK, MessageBoxImage.Warning);
		}
	}

	private bool InvokeAsAdmin(ProcessStartInfo psi)
	{
		psi.UseShellExecute = true;
		psi.Verb = "runas";
		psi.CreateNoWindow = true;

		var process = Process.Start(psi);

		process.WaitForExit();

		return process.ExitCode == 0;
	}

	#endregion
}