using System;
using System.IO;
using System.Reflection;
using System.Windows;

using AnalyzerStudio.Models;

namespace AnalyzerStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		public static string Version => "v" + Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2) ?? "unknown";
		public static string Name => Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
		public static string Title => $"{Name} {Version}";

		public static new App Current => (App)Application.Current;

        private MainWindow? ProjectWindow;
        private ProjectLoadWindow ProjectLoadWindow { get; } = new ProjectLoadWindow();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ProjectLoadWindow.Closed += (o, e) => Shutdown();

            if (e.Args.Length > 0)
            {
                var path = e.Args[0];
                if (File.Exists(path))
                {
                    var project = AnalysisProject.OpenFrom(path);
					if (project != null)
						Open(project);

                    return;
                }

                MessageBox.Show($"No project file found at '{path}'.", "Project open failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            MainWindow = ProjectLoadWindow;
            MainWindow.Show();
        }

        public bool Open(AnalysisProject project)
        {
            if (!TryCloseProjectWindow())
                return false;

            ProjectWindow = new MainWindow(project);
            ProjectWindow.Closed += (o, e) =>
            {
                if (MainWindow != null)
                    return;

                ProjectLoadWindow.Close();
            };

            MainWindow = ProjectWindow;

            if (ProjectLoadWindow.IsVisible)
                ProjectLoadWindow.Hide();

            MainWindow.Show();

            return true;
        }

        public bool TryResetToStart()
        {
            MainWindow = ProjectLoadWindow;

            if (!TryCloseProjectWindow())
                return false;

            ProjectLoadWindow.Show();

            return true;
        }

        public bool TryCloseProjectWindow()
        {
            if (ProjectWindow == null)
                return true;

            var isClosed = false;
            void closedListener(object? o, EventArgs e) => isClosed = true;

            ProjectWindow.Closed += closedListener;
            ProjectWindow.Close();
            ProjectWindow.Closed -= closedListener;

            if (!isClosed)
                return false;

            ProjectWindow = null;

            return true;
        }
    }
}
