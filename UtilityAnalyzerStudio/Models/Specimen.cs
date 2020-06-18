using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace UtilityAnalyzerStudio.Models
{
    public class Specimen : DynamicObject, INotifyPropertyChanged, ICloneable
    {
        [JsonProperty]
        public string? Name
        {
            get => name;
            set
            {
                name = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        private string? name;

        [JsonProperty]
        public readonly Dictionary<string, object?> Properties = new Dictionary<string, object?>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public Specimen()
        {
        }

        public Specimen(IEnumerable<Property> properties)
        {
            foreach (var p in properties)
                if (p.DefaultValue != null)
                    Properties[p.Name] = p.DefaultValue;
        }

        private Specimen(string? name, Dictionary<string, object?> properties)
        {
            Name = name;

            CopyValues(properties, false);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = Properties.FirstOrDefault(p => p.Key.Equals(binder.Name)).Value;

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            //var property = MainWindow.CurrentProject.Properties.First(p => p.Name.Equals(binder.Name));
            //if (property == null)
            //    return false;

            //if (!property.Type.IsInstanceOfType(value))
            //    return false;

            if (value == null)
                Properties.Remove(binder.Name);
            else
                Properties[binder.Name] = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(binder.Name));

            return true;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Specimen Clone()
        {
            return new Specimen(Name, Properties);
        }

        public void CopyValues(Specimen from, bool notifyChanges = true)
        {
            Name = from.Name;

            CopyValues(from.Properties, notifyChanges);
        }

        private void CopyValues(Dictionary<string, object?> properties, bool notifyChanges)
        {
            foreach (var pair in properties)
            {
                if (pair.Value is ICloneable cloneable)
                    Properties[pair.Key] = cloneable.Clone();
                else
                    Properties[pair.Key] = pair.Value;

				if (notifyChanges)
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pair.Key));
            }
        }
    }
}
