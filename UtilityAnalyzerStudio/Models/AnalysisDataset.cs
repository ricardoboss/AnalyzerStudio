﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilityAnalyzerStudio.Models
{
    public class AnalysisDataset : BaseModel
    {
        private Specimen specimen;
        public Specimen Specimen
        {
            get => specimen;
            set => SetProperty(ref specimen, value);
        }

        public double Value {
            get => value;
            private set => SetProperty(ref this.value, value);
        }
        private double value;

        public AnalysisDataset(Specimen specimen)
        {
            Specimen = specimen;
        }

        public void UpdateValue(IEnumerable<Specimen> specimens, IEnumerable<Property> properties)
        {
            var weightedPropertyValues = new List<double>();
            var weightSum = 0d;

            foreach (var p in properties)
            {
                var allValues = specimens
                    .Where(s => s.Properties.ContainsKey(p.Name))
                    .Select(s => s.Properties[p.Name]);

                if (!specimen.Properties.TryGetValue(p.Name, out var value))
                    continue;

                p.Normalize(ref value, allValues);

                value *= p.Weight;
                weightSum += p.Weight;

                weightedPropertyValues.Add(value);
            }

            Value = weightedPropertyValues.Sum() / weightSum;
        }
    }
}