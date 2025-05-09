using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerStudio.Models;

public class AnalysisDataset : BaseModel
{
	private int rank;
	public int Rank
	{
		get => rank;
		set => SetProperty(ref rank, value);
	}

	private Specimen specimen;
	public Specimen Specimen
	{
		get => specimen;
		set => SetProperty(ref specimen, value);
	}

	public double Value
	{
		get => value;
		private set => SetProperty(ref this.value, value);
	}
	private double value;

	public AnalysisDataset(Specimen specimen)
	{
		this.specimen = specimen;
	}

	public void UpdateValue(IReadOnlyCollection<Specimen> specimens, IReadOnlyCollection<Property> properties)
	{
		var weightedPropertyValues = new List<double>();
		var weightSum = 0d;

		foreach (var p in properties)
		{
			if (p.Type.Equals(PropertyType.Text))
				continue; // text properties are not evaluated

			var allValues = specimens
				.Where(s => s.Properties.ContainsKey(p.Name))
				.Select(s => Convert.ToDouble(s.Properties[p.Name]));

			if (!specimen.Properties.TryGetValue(p.Name, out var rawValue))
				continue;

			var normalizedValue = Convert.ToDouble(rawValue);

			normalizedValue = p.Normalize(normalizedValue, allValues.ToList());
			normalizedValue *= p.Weight;
			weightSum += p.Weight;

			weightedPropertyValues.Add(normalizedValue);
		}

		Value = weightedPropertyValues.Sum() / weightSum;
	}
}
