using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace UtilityAnalyzerStudio.Rules
{
    class ParseableDoubleRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "Value is null");

            if (!double.TryParse(value.ToString(), out _))
                return new ValidationResult(false, "Invalid double: " + value.ToString());

            return ValidationResult.ValidResult;
        }
    }
}
