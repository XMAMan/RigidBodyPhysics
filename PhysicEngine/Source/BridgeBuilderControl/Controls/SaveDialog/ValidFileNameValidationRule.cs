using System;
using System.Globalization;
using System.Windows.Controls;

namespace BridgeBuilderControl.Controls.SaveDialog
{
    //https://learn.microsoft.com/de-de/dotnet/desktop/wpf/data/how-to-implement-binding-validation?view=netframeworkdesktop-4.8
    public class ValidFileNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (((string)value).Length > 0)
                {
                    string text = (string)value;

                    if (FileNameChecker.IsValidFileName(text) == false)
                        return new ValidationResult(false, $"Illegal character {FileNameChecker.GetFistInvalidCharacter(text)}");
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or {e.Message}");
            }


            return ValidationResult.ValidResult;
        }
    }
}
