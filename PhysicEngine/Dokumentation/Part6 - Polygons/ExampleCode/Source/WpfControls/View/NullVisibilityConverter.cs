using System;
using System.Windows.Data;
using System.Windows;

namespace WpfControls.View
{
    //Ein Element (ContentControl) soll nur dann sichtbar sein, wenn eine Variable (ViewModel) != null ist
    //https://stackoverflow.com/questions/21939667/nulltovisibilityconverter-make-visible-if-not-null
    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string defaultInvisibility = parameter as string;
            Visibility invisibility = (defaultInvisibility != null) ?
                (Visibility)Enum.Parse(typeof(Visibility), defaultInvisibility)
                : Visibility.Collapsed;
            return value == null ? invisibility : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
