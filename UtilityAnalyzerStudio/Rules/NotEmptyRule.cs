using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace UtilityAnalyzerStudio.Rules
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
