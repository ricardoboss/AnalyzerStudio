using System.Globalization;
using System.Windows.Controls;

namespace AnalyzerStudio.Rules
{
    public class NotEmptyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string text))
                return ValidationResult.ValidResult;

            return new ValidationResult(text.Length > 0, "Text cannot be empty!");
        }
    }
}
