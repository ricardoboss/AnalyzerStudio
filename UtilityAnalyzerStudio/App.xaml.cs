using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using UtilityAnalyzerStudio.Models;

namespace UtilityAnalyzerStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => (App) Application.Current;

        private MainWindow ProjectWindow;
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
                    Open(project);

                    return;
                }
                else
                    MessageBox.Show($"No project file found at '{path}'.", "Project open failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            MainWindow = ProjectLoadWindow;
            MainWindow.Show();
        }

        public bool Open(AnalysisProject project)
        {
            if (!TryCloseProjectWindow())
                return false;

            ProjectWindow = new MainWindow();
            ProjectWindow.LoadProject(project);
            ProjectWindow.Closed += (o, e) => {
                if (MainWindow == null)
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

            ProjectWindow.Closed += (o, e) => isClosed = true;
            ProjectWindow.Close();

            if (!isClosed)
                return false;

            ProjectWindow = null;

            return true;
        }
    }
}
