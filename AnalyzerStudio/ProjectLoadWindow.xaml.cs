using Microsoft.Win32;

using System.Windows;

using AnalyzerStudio.Models;

namespace AnalyzerStudio
{
	/// <summary>
	/// Interaction logic for ProjectLoadWindow.xaml
	/// </summary>
	public partial class ProjectLoadWindow
	{
		public ProjectLoadWindow()
		{
			InitializeComponent();
		}

		private void ButtonNew_Click(object sender, RoutedEventArgs e)
		{
			string name = "";
			if (!TryGetProjectName(this, ref name))
				return;

			var path = "";
			if (!TryGetNewProjectPath(this, name, ref path))
				return;

			var project = AnalysisProject.NewAt(name, path);
			App.Current.Open(project);
		}

		public static bool TryGetProjectName(Window owner, ref string name, bool newName = false)
		{
			do
			{
				var prompt = new PromptWindow("name", "Project Name")
				{
					Owner = owner,
					Title = newName ? "Renaming Project" : "New Project",
					Description = newName ? "What is the new name for your project?" : "What is the name of your new project?"
				};
				var result = prompt.ShowDialog();
				if (!result.HasValue || !result.Value)
					return false;

				name = prompt.InputValues["name"];
				if (name.Length > 0)
					break;

				MessageBox.Show("Please enter a project name!", prompt.Title, MessageBoxButton.OK, MessageBoxImage.Error);
			} while (name.Length == 0);

			return true;
		}

		public static bool TryGetNewProjectPath(Window owner, string name, ref string path)
		{
			var sfd = new SaveFileDialog
			{
				FileName = name,
				AddExtension = true,
				DefaultExt = App.ProjectFileExtension,
				Filter = App.ProjectFileFilter
			};

			var result = sfd.ShowDialog(owner);
			if (!result.HasValue || !result.Value)
				return false;

			path = sfd.FileName;

			return true;
		}

		private void ButtonLoad_Click(object sender, RoutedEventArgs e)
		{
			var path = "";
			if (!TryGetExistingProjectPath(this, ref path))
				return;

			var project = AnalysisProject.OpenFrom(path);
			if (project == null)
				return;

			App.Current.Open(project);
		}

		public static bool TryGetExistingProjectPath(Window owner, ref string path)
		{
			var ofd = new OpenFileDialog
			{
				DefaultExt = App.ProjectFileExtension,
				Filter = App.ProjectFileFilter
			};

			var result = ofd.ShowDialog(owner);
			if (!result.HasValue || !result.Value)
				return false;

			path = ofd.FileName;

			return true;
		}

		private void HyperlinkAbout_Click(object sender, RoutedEventArgs e)
		{
			var aboutWindow = new AboutWindow
			{
				Owner = this
			};
			aboutWindow.Show();
		}
	}
}
