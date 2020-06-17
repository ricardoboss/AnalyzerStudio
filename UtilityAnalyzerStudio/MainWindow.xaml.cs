using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using UtilityAnalyzerStudio.Extensions;
using UtilityAnalyzerStudio.Models;

namespace UtilityAnalyzerStudio
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public static RoutedCommand AppExitCommand = new RoutedCommand();
		public static RoutedCommand ProjectChangeNameCommand = new RoutedCommand();
		public static RoutedCommand SpecimenCreateCommand = new RoutedCommand();
		public static RoutedCommand PropertyCreateCommand = new RoutedCommand();

		public event PropertyChangedEventHandler PropertyChanged;

		private AnalysisProject currentProject;
		public AnalysisProject CurrentProject
		{
			get => currentProject;
			private set
			{
				currentProject = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProject)));
			}
		}

		private Specimen selectedSpecimen;
		public Specimen SelectedSpecimen
		{
			get => selectedSpecimen;
			set
			{
				selectedSpecimen = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSpecimen)));
			}
		}

		private Property selectedProperty;
		public Property SelectedProperty
		{
			get => selectedProperty;
			set
			{
				selectedProperty = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProperty)));
			}
		}

		private AnalysisDataset selectedDataset;
		public AnalysisDataset SelectedDataset
		{
			get => selectedDataset;
			set
			{
				selectedDataset = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDataset)));
			}
		}

		private TabItem selectedTab;
		public TabItem SelectedTab
		{
			get => selectedTab;
			set
			{
				selectedTab = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTab)));
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			DataContext = this;

			Closing += MainWindow_Closing;

			SelectedTab = (TabItem) TabControlMain.Items[0];
		}

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

		public void LoadProject(AnalysisProject project)
		{
			if (CurrentProject != null)
			{
				CurrentProject.Properties.CollectionChanged -= Properties_CollectionChanged;

				UpdateSpecimensColumns(CurrentProject.Properties, new List<Property>());
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
					Path = new PropertyPath(nameof(Property.Name))
				};
				var headerTemplate = new DataTemplate();
				var headerTextBlock = new FrameworkElementFactory(typeof(TextBlock));
				headerTextBlock.SetBinding(TextBlock.TextProperty, headerBinding);
				headerTemplate.VisualTree = headerTextBlock;

				var cellBinding = new Binding
				{
					Path = new PropertyPath(property.Name)
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

		private void ButtonAddProperty_Click(object sender, RoutedEventArgs e)
		{
			CreateProperty();
		}

		private void CreateProperty()
		{
			var property = new Property();
			if (!Edit(ref property))
				return;

			CurrentProject.Properties.Add(property);
		}

		private void ButtonAddSpecimen_Click(object sender, RoutedEventArgs e)
		{
			CreateSpecimen();
		}

		private void CreateSpecimen()
		{
			var editor = new SpecimenEditorWindow(CurrentProject.Properties)
			{
				Owner = this
			};
			var specimen = new Specimen(CurrentProject.Properties);

			var saved = editor.Edit(ref specimen);
			if (saved)
				CurrentProject.Specimens.Add(specimen);
		}

		private void ListViewSpecimensItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListViewItem lvi) || !(lvi.Content is Specimen original))
				return;

			var editorWindow = new SpecimenEditorWindow(CurrentProject.Properties)
			{
				Owner = this
			};

			var clone = original.Clone();
			if (!editorWindow.Edit(ref clone))
				return;

			original.CopyValues(clone);
		}

		private void ListViewPropertiesItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListViewItem lvi) || !(lvi.Content is Property original))
				return;

			var clone = original.Clone();
			if (!Edit(ref clone))
				return;

			original.CopyValuesFrom(clone);
		}

		private bool Edit(ref Property property)
		{
			var editorWindow = new PropertyEditorWindow(property)
			{
				Owner = this
			};

			var result = editorWindow.ShowDialog();

			return result.HasValue && result.Value;
		}

		#region Commands

		private void ProjectSaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentProject.IsDirty;
		}

		private void ProjectSaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CurrentProject.Save();
		}

		private void ProjectSaveAtCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void ProjectSaveAtCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var path = "";
			if (!ProjectLoadWindow.TryGetNewProjectPath(this, CurrentProject.Name, ref path))
				return;

			CurrentProject.Save(path);

			MessageBox.Show("Successfully saved project at " + path, "Project Saved");
		}

		private void ProjectCloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void ProjectCloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			App.Current.TryResetToStart();
		}

		private void ProjectChangeNameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void ProjectChangeNameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var name = "";
			if (ProjectLoadWindow.TryGetProjectName(this, ref name, true))
				CurrentProject.Name = name;
		}

		private void AppExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void AppExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		private void SpecimenCreateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void SpecimenCreateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CreateSpecimen();
		}

		private void PropertyCreateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void PropertyCreateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CreateProperty();
		}

		private void DeleteSelectedCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = false;

			if (SelectedTab == null)
				return;

			if (SelectedTab.Tag.Equals(typeof(Specimen)))
			{
				if (SelectedSpecimen == null)
					return;
			}
			else if (SelectedTab.Tag.Equals(typeof(Property)))
			{
				if (SelectedProperty == null)
					return;
			}

			e.CanExecute = true;
		}

		private void DeleteSelectedCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (SelectedTab.Tag.Equals(typeof(Specimen)))
			{
				if (SelectedSpecimen == null)
					return;

				var result = MessageBox.Show($"Are you sure you want to delete the specimen '{SelectedSpecimen.Name}'?", "Deleting Property", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				if (!result.Equals(MessageBoxResult.Yes))
					return;

				CurrentProject.Specimens.Remove(SelectedSpecimen);
			}
			else if (SelectedTab.Tag.Equals(typeof(Property)))
			{
				if (SelectedProperty == null)
					return;

				var result = MessageBox.Show($"Are you sure you want to delete the property '{SelectedProperty.Name}'? This will also remove it from every specimen.", "Deleting Property", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				if (!result.Equals(MessageBoxResult.Yes))
					return;

				CurrentProject.Properties.Remove(SelectedProperty);
			}
		}

		#endregion
	}
}
