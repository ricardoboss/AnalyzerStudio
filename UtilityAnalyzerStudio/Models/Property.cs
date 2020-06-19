using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AnalyzerStudio.Models
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

	public class PropertyNameChangedEventArgs : EventArgs
	{
		public string? NewName { get; }
		public string? OldName { get; }

		public bool Canceled { get; set; }

		public PropertyNameChangedEventArgs(string? newName, string? oldName)
		{
			NewName = newName;
			OldName = oldName;
		}
	}

	public class PropertyTypeChangedEventArgs : EventArgs
	{
		public PropertyType NewType { get; }
		public PropertyType OldType { get; }

		public bool Canceled { get; set; }

		public PropertyTypeChangedEventArgs(PropertyType newType, PropertyType oldType)
		{
			NewType = newType;
			OldType = oldType;
		}
	}

    public class Property : BaseModel, ICloneable
    {
		public static bool TryConvert(PropertyType fromType, PropertyType toType, object? value, out object? convertedValue)
		{
			if (fromType.Equals(toType))
			{
				convertedValue = value;

				return true;
			}

			switch (toType)
			{
				case PropertyType.Text:
					convertedValue = Convert.ToString(value, CultureInfo.CurrentCulture);

					return true;
				case PropertyType.Double:
					convertedValue = Convert.ToDouble(value, CultureInfo.CurrentCulture);

					return true;
				case PropertyType.Boolean:
					convertedValue = Convert.ToBoolean(value, CultureInfo.CurrentCulture);

					return true;
				default:
					throw new NotImplementedException();
			}
		}

		public event EventHandler<PropertyNameChangedEventArgs>? NameChanged;
		public event EventHandler<PropertyTypeChangedEventArgs>? TypeChanged;

		[JsonProperty]
		public string Name
		{
			get => name ?? ""; set
			{
				if (name == value)
					return;

				var args = new PropertyNameChangedEventArgs(value, name);
				NameChanged?.Invoke(this, args);

				if (args.Canceled)
					return;

				SetProperty(ref name, value);
			}
		}
        private string? name;

        [JsonProperty]
        public int Weight { get => weight; set => SetProperty(ref weight, value); }
        private int weight;

        [JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public PropertyType Type
		{
			get => type;
			set
			{
				if (type.Equals(value))
					return;

				var args = new PropertyTypeChangedEventArgs(value, type);
				TypeChanged?.Invoke(this, args);

				if (args.Canceled)
					return;

				SetProperty(ref type, value);
			}
		}
        private PropertyType type = PropertyType.Text;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public NormalizationStrategy NormalizationStrategy { get => normalizationStrategy; set => SetProperty(ref normalizationStrategy, value); }
        private NormalizationStrategy normalizationStrategy = NormalizationStrategy.Max;

        [JsonIgnore]
        public object? DefaultValue => Type switch
        {
            PropertyType.Boolean => false,
            PropertyType.Double => 0d,
            PropertyType.Text => "",
            _ => null
        };

		public override string ToString() => Name;

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

        public void CopyValues(Property property)
        {
            Name = property.Name;
            Weight = property.Weight;
            Type = property.Type;
            NormalizationStrategy = property.NormalizationStrategy;
        }
    }
}
