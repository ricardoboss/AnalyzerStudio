using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace UtilityAnalyzerStudio.Models
{
    public enum NormalizationStrategy
    {
        Max,
        Min,
        Average
    }

    public class Property : BaseModel, ICloneable
    {
        [JsonProperty]
        public string Name { get => name; set => SetProperty(ref name, value); }
        private string name;
        
        [JsonProperty]
        public int Weight { get => weight; set => SetProperty(ref weight, value); }
        private int weight;

        public NormalizationStrategy NormalizationStrategy { get => normalizationStrategy; set => SetProperty(ref normalizationStrategy, value); }
        private NormalizationStrategy normalizationStrategy = NormalizationStrategy.Max;

        public override string ToString()
        {
            return Name + " (" + Weight + ")";
        }

        public void Normalize(ref double value, IEnumerable<double> allValues)
        {
            switch (NormalizationStrategy)
            {
                case NormalizationStrategy.Max:

                    var max = allValues.Max();

                    value /= max;
                    break;
                case NormalizationStrategy.Min:
                    var min = allValues.Min();

                    value = min / value;
                    break;
                case NormalizationStrategy.Average:
                    var avg = allValues.Average();

                    value = 1 - Math.Abs((2 / (1 + Math.Exp(value - avg))) - 1);
                    break;
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Property Clone()
        {
            return new Property
            {
                Name = Name,
                Weight = Weight,
                NormalizationStrategy = NormalizationStrategy
            };
        }
    }
}
