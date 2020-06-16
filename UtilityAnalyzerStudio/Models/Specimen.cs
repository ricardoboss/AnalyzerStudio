using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace UtilityAnalyzerStudio.Models
{
    public class Specimen : DynamicObject, INotifyPropertyChanged
    {
        public static void CopyProperties(Specimen to, Specimen from)
        {
            foreach (var pair in from.Properties)
            {
                to.Properties[pair.Key] = pair.Value;
                to.PropertyChanged?.Invoke(to, new PropertyChangedEventArgs(pair.Key));
            }
        }

        [JsonProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        private string name;

        [JsonProperty]
        public readonly Dictionary<string, double> Properties = new Dictionary<string, double>();

        public event PropertyChangedEventHandler PropertyChanged;
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = Properties.FirstOrDefault(p => p.Key.Equals(binder.Name)).Value;

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            //var property = MainWindow.CurrentProject.Properties.First(p => p.Name.Equals(binder.Name));
            //if (property == null)
            //    return false;

            //if (!property.Type.IsInstanceOfType(value))
            //    return false;

            if (value == null)
                Properties.Remove(binder.Name);
            else
                Properties[binder.Name] = double.Parse(value.ToString());

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(binder.Name));

            return true;
        }
    }
}
