using System;
using System.IO;
using System.Reflection;
using System.Windows;

using AnalyzerStudio.Extensions;
using AnalyzerStudio.Models;

namespace AnalyzerStudio;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	public static string VersionNumber => Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2) ?? "?";
	public static string Version => "v" + Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2) ?? "unknown";
	public static string Name => Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
	public static string Title => $"{Name} {Version}";
	public static string ProjectFileExtension => "asproj";
	public static string ProjectFileFilter => $"Analyzer Studio Project File|*.{ProjectFileExtension}|JSON Project File|*.json";

	public static new App Current => (App)Application.Current;

	private MainWindow? ProjectWindow;
	internal ProjectLoadWindow? ProjectLoadWindow { get; private set; }

	private void Application_Startup(object sender, StartupEventArgs e)
	{
		RegistryManager.Init();

		ProjectLoadWindow = new();
		ProjectLoadWindow.Closed += (o, e) => Shutdown();

		if (e.Args.Length > 0)
		{
			var firstArg = e.Args[0];
			switch (firstArg)
			{
				case "--install-extension":
					RegistryManager.InstallExtension();
					Shutdown();
					return;

				case "--uninstall-extension":
					RegistryManager.UninstallExtension();
					Shutdown();
					return;

				default:
					if (OpenFromCLI(firstArg))
						return;

					break;
			}
		}

		MainWindow = ProjectLoadWindow;
		MainWindow.Show();
	}

	private void Application_Exit(object sender, ExitEventArgs e)
	{
		RegistryManager.SetFirstRan();
	}

	private bool OpenFromCLI(string path)
	{
		if (!File.Exists(path))
		{
			MessageBox.Show($"No project file found at '{path}'.", "Project open failed", MessageBoxButton.OK, MessageBoxImage.Error);
			return false;
		}

		var project = AnalysisProject.OpenFrom(path);
		if (project == null)
			return false;

		return Open(project);
	}

	public bool Open(AnalysisProject project)
	{
		if (!TryCloseProjectWindow())
			return false;

		ProjectWindow = new(project);
		ProjectWindow.Closed += (o, e) =>
		{
			if (MainWindow != null)
				return;

			ProjectLoadWindow?.Close();
		};

		MainWindow = ProjectWindow;

		if (ProjectLoadWindow?.IsVisible ?? false)
			ProjectLoadWindow.Hide();

		MainWindow.Show();

		return true;
	}

	public bool TryCloseCurrent()
	{
		MainWindow = ProjectLoadWindow;

		if (!TryCloseProjectWindow())
			return false;

		ProjectLoadWindow?.Show();

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