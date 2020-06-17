using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UtilityAnalyzerStudio.Models
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null, params string[] propertyNames)
        {
            property = value;

            var properties = new List<string>(propertyNames) { propertyName };

            foreach (var prop in properties)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
