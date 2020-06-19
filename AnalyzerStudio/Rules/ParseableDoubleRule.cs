using System.Globalization;
using System.Windows.Controls;

namespace AnalyzerStudio.Rules
{
    class ParseableDoubleRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || value.ToString()!.Length == 0)
                return new ValidationResult(false, "Value is empty");

            if (!double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out _))
                return new ValidationResult(false, "Invalid number: " + value.ToString());

            return ValidationResult.ValidResult;
        }
    }
}
