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
            if (value == null || value.ToString().Length == 0)
                return new ValidationResult(false, "Value is empty");

            if (!double.TryParse(value.ToString(), out _))
                return new ValidationResult(false, "Invalid number: " + value.ToString());

            return ValidationResult.ValidResult;
        }
    }
}
