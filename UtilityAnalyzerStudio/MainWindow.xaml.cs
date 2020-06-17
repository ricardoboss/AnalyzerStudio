using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using UtilityAnalyzerStudio.Models;

namespace UtilityAnalyzerStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AnalysisProject CurrentProject
        {
            get => currentProject;
            private set
            {
                currentProject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProject)));
            }
        }
        private AnalysisProject currentProject;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Closing += MainWindow_Closing;
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
                CurrentProject.Specimens.CollectionChanged -= Specimens_CollectionChanged;
                CurrentProject.Properties.CollectionChanged -= Properties_CollectionChanged;

                UpdateSpecimensColumns(CurrentProject.Properties, new List<Property>());
            }

            project.Specimens.CollectionChanged += Specimens_CollectionChanged;
            project.Properties.CollectionChanged += Properties_CollectionChanged;

            UpdateSpecimensColumns(new List<Property>(), project.Properties);

            project.UpdateAnalysis();

            CurrentProject = project;
        }

        private void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (Property property in e.OldItems)
                    property.PropertyChanged -= Property_PropertyChanged;

            if (e.NewItems != null)
                foreach (Property property in e.NewItems)
                    property.PropertyChanged += Property_PropertyChanged;

            UpdateSpecimensColumns((e.OldItems ?? new List<Property>()).Cast<Property>(), (e.NewItems ?? new List<Property>()).Cast<Property>());

            CurrentProject.UpdateAnalysis();
        }

        private void Specimens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (Specimen specimen in e.OldItems)
                    specimen.PropertyChanged -= Specimen_PropertyChanged;

            if (e.NewItems != null)
                foreach (Specimen specimen in e.NewItems)
                    specimen.PropertyChanged += Specimen_PropertyChanged;

            CurrentProject.UpdateAnalysis();
        }

        private void Property_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CurrentProject.UpdateAnalysis();
        }

        private void Specimen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CurrentProject.UpdateAnalysis();
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
                    CellTemplate = cellTemplate
                };
                SpecimensGridView.Columns.Add(col);
            }
        }

        private void ButtonAddProperty_Click(object sender, RoutedEventArgs e)
        {
            var property = new Property();
            if (!Edit(ref property))
                return;

            CurrentProject.Properties.Add(property);
        }

        private void ButtonAddSpecimen_Click(object sender, RoutedEventArgs e)
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

        private void MenuItemSaveProject_Click(object sender, RoutedEventArgs e)
        {
            CurrentProject.Save();
        }

        private void MenuItemSaveProjectAt_Click(object sender, RoutedEventArgs e)
        {
            var path = "";
            if (!ProjectLoadWindow.TryGetNewProjectPath(this, CurrentProject.Name, ref path))
                return;

            CurrentProject.Save(path);

            MessageBox.Show("Successfully saved project at " + path, "Project Saved");
        }

        private void MenuItemCloseProject_Click(object sender, RoutedEventArgs e)
        {
            App.Current.TryResetToStart();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemChangeProjectName_Click(object sender, RoutedEventArgs e)
        {
            var name = "";
            if (ProjectLoadWindow.TryGetProjectName(this, ref name, true))
                CurrentProject.Name = name;
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
    }
}
