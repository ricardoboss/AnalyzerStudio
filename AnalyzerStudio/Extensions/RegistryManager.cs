using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Text;

namespace AnalyzerStudio.Extensions
{
	internal static class RegistryManager
	{
		private const string ProgID = "AnalyzerStudio";
		private const string Key_ProjectFileExtension = @"Software\Classes\.asproj";
		private const string Key_JsonFileExtensionOpenWith = @"Software\Classes\.json\OpenWithProgids";

		private static readonly string KeyProgram = $@"Software\{ProgID}";
		private static readonly string Key_ProgramClassRoot = $@"Software\Classes\{ProgID}";
		private static readonly string Key_OpenCommand = $@"shell\open\command";
		private static readonly RegistryKey RootKey = Registry.CurrentUser;

		private static RegistryKey? ProgramKey { get; set; }

		private static string ExeLocation => Process.GetCurrentProcess().MainModule.FileName;
		private static string IconLocation => ExeLocation;
		private static string CommandLine => $"\"{ExeLocation}\" \"%1\"";

		public static void Init()
		{
			ProgramKey = RootKey.CreateSubKey(KeyProgram);
		}

		public static bool IsFirstRun()
		{
			return ProgramKey == null ||
				ProgramKey.ValueCount == 0 ||
				((bool)ProgramKey.GetValue("AlreadyRan", false)) == false;
		}

		public static void SetFirstRan()
		{
			ProgramKey?.SetValue("AlreadyRan", true);
		}

		public static void InstallExtension()
		{
			// create program class
			var programClassRoot = RootKey.CreateSubKey(Key_ProgramClassRoot);

			// set default icon for files registered with this program
			var programIcon = programClassRoot.CreateSubKey("DefaultIcon");
			programIcon.SetValue(string.Empty, IconLocation);

			// set "open" command
			var programCommandLine = programClassRoot.CreateSubKey(Key_OpenCommand);
			programCommandLine.SetValue(string.Empty, CommandLine);

			// create file class
			var projectFileClassRoot = RootKey.CreateSubKey(Key_ProjectFileExtension);

			// add programm class as default handler
			projectFileClassRoot.SetValue(string.Empty, ProgID);

			// add program class to "open with" alternatives in case default handler gets changed
			var projectFileOpenWith = projectFileClassRoot.CreateSubKey("OpenWithProgids");
			projectFileOpenWith.SetValue(ProgID, string.Empty);

			// add program class to .json files' "open with" alternatives
			var jsonFileOpenWith = RootKey.CreateSubKey(Key_JsonFileExtensionOpenWith);
			jsonFileOpenWith.SetValue(ProgID, string.Empty);
		}

		public static void UninstallExtension()
		{
			// remove program class from .json files' "open with" alternatives
			var jsonFileOpenWith = RootKey.OpenSubKey(Key_JsonFileExtensionOpenWith);
			jsonFileOpenWith.DeleteValue(ProgID);

			// remove default handler for file class
			var projectFileClassRoot = RootKey.OpenSubKey(Key_ProjectFileExtension);
			projectFileClassRoot.DeleteValue(string.Empty);

			// remove program class from "open with" alternatives of file class
			var projectFileOpenWith = projectFileClassRoot.OpenSubKey("OpenWithProgids");
			projectFileOpenWith.DeleteValue(ProgID);

			// delete program class tree
			RootKey.DeleteSubKeyTree(Key_ProgramClassRoot);
		}
	}
}
