using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityAnalyzerStudio.Models
{
    public enum NormalizationStrategy
    {
        Max,
        Min
    }

    public enum PropertyType
    {
        Text,
        Double,
        Boolean
    }

    public class Property : BaseModel, ICloneable
    {
        [JsonProperty]
        public string Name { get => name; set => SetProperty(ref name, value); }
        private string name;

        [JsonProperty]
        public int Weight { get => weight; set => SetProperty(ref weight, value); }
        private int weight;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public PropertyType Type { get => type; set => SetProperty(ref type, value); }
        private PropertyType type = PropertyType.Text;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public NormalizationStrategy NormalizationStrategy { get => normalizationStrategy; set => SetProperty(ref normalizationStrategy, value); }
        private NormalizationStrategy normalizationStrategy = NormalizationStrategy.Max;

        [JsonIgnore]
        public object DefaultValue => Type switch
        {
            PropertyType.Boolean => false,
            PropertyType.Double => 0d,
            PropertyType.Text => "",
            _ => null
        };

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
                    if (max == 0)
                        break;

                    value /= max;
                    break;
                case NormalizationStrategy.Min:
                    var min = allValues.Min();
                    if (value == 0)
                        break;

                    value = min / value;
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
                Type = Type,
                NormalizationStrategy = NormalizationStrategy
            };
        }

        public void CopyValuesFrom(Property property)
        {
            Name = property.Name;
            Weight = property.Weight;
            Type = property.Type;
            NormalizationStrategy = property.NormalizationStrategy;
        }
    }
}
