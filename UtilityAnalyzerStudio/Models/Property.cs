using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace UtilityAnalyzerStudio.Models
{
    public class Property : BaseModel
    {
        [JsonProperty]
        public string Name { get => name; set => SetProperty(ref name, value); }
        private string name;
        
        [JsonProperty]
        public int Weight { get => weight; set => SetProperty(ref weight, value); }
        private int weight;

        public override string ToString()
        {
            return Name + " (" + Weight + ")";
        }
    }
}
