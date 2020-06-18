using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace UtilityAnalyzerStudio.Models
{
	public class AnalysisProject : BaseModel
	{
		private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore
		};

		public static AnalysisProject NewAt(string name, string path)
		{
			var project = new AnalysisProject(name)
			{
				Path = path,
				IsDirty = true
			};

			return project;
		}

		public static void SaveAt(AnalysisProject project, string path)
		{
			var json = JsonConvert.SerializeObject(project, settings);
			File.WriteAllText(path, json);

			project.Path = path;
			project.IsDirty = false;
		}

		public static AnalysisProject? OpenFrom(string path)
		{
			var json = File.ReadAllText(path);

			var settings = AnalysisProject.settings;
			string? errorMessage = null;
			settings.Error = (o, e) => errorMessage = e.ErrorContext.Error.Message;

			var project = JsonConvert.DeserializeObject<AnalysisProject>(json, settings);
			if (project == null)
			{
				if (errorMessage != null)
					MessageBox.Show($"Failed to load project: {errorMessage}", "Open Project Failed", MessageBoxButton.OK);
				else
					MessageBox.Show("Failed to load project.", "Open Project Failed", MessageBoxButton.OK);

				return null;
			}

			project.Path = path;

			return project;
		}

		[JsonIgnore]
		public bool IsDirty
		{
			get => isDirty;
			private set
			{
				SetProperty(ref isDirty, value, nameof(IsDirty), nameof(ProjectTitle));

				UpdateAnalysis();
			}
		}
		private bool isDirty;

		[JsonIgnore]
		public string Path
		{
			get => path ?? throw new NullReferenceException();
			private set => SetProperty(ref path, value, nameof(Path), nameof(ProjectTitle));
		}
		private string? path;

		[JsonIgnore]
		public string ProjectTitle => $"{Name}{(IsDirty ? "*" : "")} ({Path})";

		[JsonProperty]
		public string Name
		{
			get => name ?? "";
			set
			{
				SetProperty(ref name, value, nameof(Name), nameof(ProjectTitle));
				IsDirty = true;
			}
		}
		private string? name;

		[JsonProperty]
		public ObservableCollection<Property> Properties { get; } = new ObservableCollection<Property>();

		[JsonProperty]
		public ObservableCollection<Specimen> Specimens { get; } = new ObservableCollection<Specimen>();

		[JsonIgnore]
		public ObservableCollection<AnalysisDataset> Datasets { get; } = new ObservableCollection<AnalysisDataset>();

		private AnalysisProject(string name)
		{
			Name = name;

			Properties.CollectionChanged += Properties_CollectionChanged;
			Specimens.CollectionChanged += Specimens_CollectionChanged;
		}

		[JsonConstructor]
		public AnalysisProject(string name, IEnumerable<Property> properties, IEnumerable<Specimen> specimens) : this(name)
		{
			foreach (var p in properties)
				Properties.Add(p);

			foreach (var s in specimens)
				Specimens.Add(s);

			IsDirty = false;
		}

		private void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
				foreach (Property? p in e.NewItems)
				{
					if (p == null)
						continue;

					p.PropertyChanged += Property_PropertyChanged;
					p.NameChanged += Property_NameChanged;
					p.TypeChanged += Property_TypeChanged;

					foreach (var s in Specimens)
						s.Properties[p.Name] = p.DefaultValue;
				}

			if (e.OldItems != null)
				foreach (Property? p in e.OldItems)
				{
					if (p == null)
						continue;

					p.PropertyChanged -= Property_PropertyChanged;
					p.NameChanged -= Property_NameChanged;
					p.TypeChanged -= Property_TypeChanged;

					foreach (var s in Specimens)
						s.Properties.Remove(p.Name);
				}

			IsDirty = true;
		}

		private void Property_TypeChanged(object? sender, PropertyTypeChangedEventArgs e)
		{
			if (!(sender is Property p))
				return;

			var clones = new Dictionary<Specimen, Specimen>();
			foreach (var os in Specimens)
			{
				var s = os.Clone();
				if (!s.Properties.ContainsKey(p.Name))
				{
					s.Properties[p.Name] = p.DefaultValue;

					continue;
				}

				var oldValue = s.Properties[p.Name];
				if (!Property.TryConvert(e.OldType, e.NewType, oldValue, out var newValue))
				{
					MessageBox.Show($"Failed to convert property '{p.Name}' of specimen '{s.Name}' to the new type '{e.NewType}'!");

					e.Canceled = true;
					return;
				}

				s.Properties[p.Name] = newValue;

				clones[os] = s;
			}

			foreach (var pair in clones)
				pair.Key.CopyValues(pair.Value, false);
		}

		private void Property_NameChanged(object? sender, PropertyNameChangedEventArgs e)
		{
			if (e.OldName == null || e.NewName == null)
				return;

			var clones = new Dictionary<Specimen, Specimen>();
			foreach (var os in Specimens)
			{
				var s = os.Clone();
				if (s.Properties.ContainsKey(e.NewName))
				{
					MessageBox.Show($"A property with the name '{e.NewName}' is already set on specimen '{s.Name}'!");

					e.Canceled = true;
					return;
				}

				s.Properties.Remove(e.OldName, out var value);
				s.Properties[e.NewName] = value;

				clones[os] = s;
			}

			foreach (var pair in clones)
				pair.Key.CopyValues(pair.Value, false);
		}

		private void Specimens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
				foreach (Specimen? s in e.NewItems)
					if (s != null)
						s.PropertyChanged += Specimen_PropertyChanged;

			if (e.OldItems != null)
				foreach (Specimen? s in e.OldItems)
					if (s != null)
						s.PropertyChanged -= Specimen_PropertyChanged;

			IsDirty = true;
		}

		private void Property_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IsDirty = true;
		}

		private void Specimen_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IsDirty = true;
		}

		public void Save(string? path = null)
		{
			SaveAt(this, path ?? Path);
		}

		public void UpdateAnalysis()
		{
			var seenSpecimen = new List<Specimen>();

			foreach (var ds in Datasets)
			{
				ds.UpdateValue(Specimens, Properties);

				seenSpecimen.Add(ds.Specimen);
			}

			foreach (var specimen in Specimens.Where(s => !seenSpecimen.Contains(s)))
			{
				var ds = new AnalysisDataset(specimen);

				ds.UpdateValue(Specimens, Properties);

				Datasets.Add(ds);
			}

			foreach (var specimen in seenSpecimen.Where(s => !Specimens.Contains(s)))
			{
				var ds = Datasets.First(d => d.Specimen == specimen);
				if (ds == null)
					continue;

				Datasets.Remove(ds);
			}
		}
	}
}
