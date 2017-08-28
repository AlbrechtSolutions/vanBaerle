using System;
using System.IO;
using System.Web;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dna.Ecommerce.LiveIntegration.Logging;

namespace Dna.Ecommerce.LiveIntegration.Configuration
{
	internal static class ConfigurationHandler
	{
		private static Settings _settings;
		private static readonly object LockObject = new object();
		private static FileSystemWatcher _fileSystemWatcher;
		private static bool _inSaveProccess = false;
		private static string ConfigFilePathVirtual => string.Format("/Files/System/LiveIntegration/{0}.Setup.xml", Constants.AddInName);
		private static string _configFilePathPhysical;
		private static string ConfigFilePathPhysical
		{
			get
			{
				if (_configFilePathPhysical == null)
				{
					_configFilePathPhysical = HttpContext.Current.Server.MapPath(ConfigFilePathVirtual); ;
				}
				return _configFilePathPhysical;
			}
		}

		static ConfigurationHandler()
		{
			EnsureConfigurationFolderAndFileExist();
			SetUpWatching();
		}
		internal static void EnsureConfigurationFolderAndFileExist()
		{
			var folder = Path.GetDirectoryName(ConfigFilePathPhysical);
			EnsureFolder(folder);
			EnsureFile(ConfigFilePathPhysical);
		}

		private static void EnsureFolder(string folder)
		{
			Directory.CreateDirectory(folder);
		}

		private static void EnsureFile(string configFilePath)
		{
			if (!File.Exists(configFilePath))
			{
				File.WriteAllText(ConfigFilePathPhysical, "<Settings></Settings>");
			}
		}

		private static void SetUpWatching()
		{
			_fileSystemWatcher = new FileSystemWatcher
			{
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
				Path = Path.GetDirectoryName(ConfigFilePathPhysical),
				Filter = Path.GetFileName(ConfigFilePathPhysical)
			};

			// Add event handlers.
			_fileSystemWatcher.Changed += OnChanged;
			_fileSystemWatcher.Deleted += OnChanged;
			_fileSystemWatcher.Renamed += OnChanged;
			_fileSystemWatcher.Error += FileSystemWatcherOnError;

			// Begin watching.
			_fileSystemWatcher.EnableRaisingEvents = true;
		}

		private static void FileSystemWatcherOnError(object sender, ErrorEventArgs errorEventArgs)
		{
			string errorMessage = string.Format("Error watching Live Integration settings XML file. Error: {0}", errorEventArgs.GetException());
			Logger.Instance.Log(ErrorLevel.Error, errorMessage);
			SetUpWatching();
		}

		private static void OnChanged(object source, FileSystemEventArgs e)
		{
			if(_inSaveProccess)
			{
				return;
			}

			_fileSystemWatcher.EnableRaisingEvents = false;
			if (!File.Exists(ConfigFilePathPhysical))
			{
				EnsureConfigurationFolderAndFileExist();
			}
			ResetSettings();
			_fileSystemWatcher.EnableRaisingEvents = true;
		}

		private static void ResetSettings()
		{
			_settings = null;
			GetSettings();
			Logger.Instance.Log(ErrorLevel.DebugInfo, "Settings reloaded because of a file watcher event.");
		}

		public static Settings GetSettings()
		{
			if (_settings == null)
			{
				_settings = LoadSettings();
			}
			return _settings;
		}

		private static Settings LoadSettings()
		{
			lock (LockObject)
			{
				var fi = new FileInfo(ConfigFilePathPhysical);
				if (!fi.Exists)
				{
					EnsureFile(ConfigFilePathPhysical);
				}
				try
				{
					return new SettingsSerializer().Deserialize(File.ReadAllText(ConfigFilePathPhysical));
				}
				catch
				{
					string errorMessage = string.Format("Error reading live integration file at {0}. Make sure the file exists, is accessible and is currently not in use.", ConfigFilePathPhysical);
					Logger.Instance.Log(ErrorLevel.Error, errorMessage);
					throw new Exception(errorMessage);
				}
			}
		}

		public static void SaveSettings()
		{
			var xml = new SettingsSerializer().Serialize(_settings);
			try
			{
				_inSaveProccess = true;
				File.WriteAllText(ConfigFilePathPhysical, xml);
				_inSaveProccess = false;
			}
			catch
			{
				string errorMessage = string.Format("Error writing live integration file at {0}. Make sure the file exists, is accessible and is currently not in use.", ConfigFilePathPhysical);
				Logger.Instance.Log(ErrorLevel.Error, errorMessage);
				throw new Exception(errorMessage);
			}
		}
	}
}
