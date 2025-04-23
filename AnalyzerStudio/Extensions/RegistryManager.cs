using Microsoft.Win32;

using System;
using System.Diagnostics;

namespace AnalyzerStudio.Extensions;

internal static class RegistryManager
{
	private const string ProgID = "AnalyzerStudio";
	private const string Key_ProjectFileExtension = @"Software\Classes\.asproj";
	private const string Key_JsonFileExtensionOpenWith = @"Software\Classes\.json\OpenWithProgids";
	private const string Key_OpenCommand = @"shell\open\command";
	private const string Key_DefaultIcon = "DefaultIcon";
	private const string Key_AlreadyRan = "AlreadyRan";

	private static readonly string Key_Program = $@"Software\{ProgID}";
	private static readonly string Key_ProgramClassRoot = $@"Software\Classes\{ProgID}";
	private static readonly RegistryKey RootKey = Registry.CurrentUser;

	private static RegistryKey? ProgramKey { get; set; }

	internal static string ExeLocation => Process.GetCurrentProcess().MainModule.FileName;
	private static string IconLocation => ExeLocation;
	private static string CommandLine => $"\"{ExeLocation}\" \"%1\"";

	public static void Init()
	{
		ProgramKey = RootKey.CreateSubKey(Key_Program);

		if (!IsFirstRun())
			UpdateExePath();
	}

	public static bool IsFirstRun()
	{
		try
		{
			return ProgramKey == null ||
			       ProgramKey.ValueCount == 0 ||
			       Convert.ToBoolean(ProgramKey.GetValue(Key_AlreadyRan, false)) == false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void SetFirstRan()
	{
		ProgramKey?.SetValue(Key_AlreadyRan, true);
	}

	public static void UpdateExePath()
	{
		var programClassRoot = RootKey.OpenSubKey(Key_ProgramClassRoot);
		if (programClassRoot == null)
			return;

		programClassRoot.OpenSubKey(Key_OpenCommand, true).SetValue(string.Empty, CommandLine);
		programClassRoot.OpenSubKey(Key_DefaultIcon, true).SetValue(string.Empty, IconLocation);
	}

	public static void InstallExtension()
	{
		// create program class
		var programClassRoot = RootKey.CreateSubKey(Key_ProgramClassRoot, true);

		// set default icon for files registered with this program
		programClassRoot.CreateSubKey(Key_DefaultIcon, true).SetValue(string.Empty, IconLocation);

		// set "open" command
		programClassRoot.CreateSubKey(Key_OpenCommand, true).SetValue(string.Empty, CommandLine);

		// create file class
		var projectFileClassRoot = RootKey.CreateSubKey(Key_ProjectFileExtension, true);

		// add programm class as default handler
		projectFileClassRoot.SetValue(string.Empty, ProgID);

		// add program class to "open with" alternatives in case default handler gets changed
		projectFileClassRoot.CreateSubKey("OpenWithProgids", true).SetValue(ProgID, string.Empty);

		// add program class to .json files' "open with" alternatives
		RootKey.CreateSubKey(Key_JsonFileExtensionOpenWith, true).SetValue(ProgID, string.Empty);
	}

	public static void UninstallExtension()
	{
		// remove program class from .json files' "open with" alternatives
		RootKey.OpenSubKey(Key_JsonFileExtensionOpenWith, true).DeleteValue(ProgID);

		// remove default handler for file class
		var projectFileClassRoot = RootKey.OpenSubKey(Key_ProjectFileExtension, true);
		projectFileClassRoot.DeleteValue(string.Empty);

		// remove program class from "open with" alternatives of file class
		projectFileClassRoot.OpenSubKey("OpenWithProgids", true).DeleteValue(ProgID);

		// delete program class tree
		RootKey.DeleteSubKeyTree(Key_ProgramClassRoot);
	}
}