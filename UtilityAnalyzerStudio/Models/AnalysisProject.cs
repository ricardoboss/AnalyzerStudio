using Newtonsoft.Json;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;

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

		public static AnalysisProject OpenFrom(string path)
		{
			var json = File.ReadAllText(path);
			var project = JsonConvert.DeserializeObject<AnalysisProject>(json, settings);
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
			get => path;
			private set => SetProperty(ref path, value, nameof(Path), nameof(ProjectTitle));
		}
		private string path;

		[JsonIgnore]
		public string ProjectTitle => $"{Name}{(IsDirty ? "*" : "")} ({Path})";

		[JsonProperty]
		public string Name
		{
			get => name;
			set
			{
				SetProperty(ref name, value, nameof(Name), nameof(ProjectTitle));
				IsDirty = true;
			}
		}
		private string name;

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
				foreach (Property p in e.NewItems)
					p.PropertyChanged += Property_PropertyChanged;

			if (e.OldItems != null)
				foreach (Property p in e.OldItems)
				{
					p.PropertyChanged -= Property_PropertyChanged;

					foreach (var s in Specimens)
						s.Properties.Remove(p.Name);
				}

			IsDirty = true;
		}

		private void Specimens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
				foreach (Specimen s in e.NewItems)
					s.PropertyChanged += Specimen_PropertyChanged;

			if (e.OldItems != null)
				foreach (Specimen s in e.NewItems)
					s.PropertyChanged += Specimen_PropertyChanged;

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

		public void Save(string path = null)
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
