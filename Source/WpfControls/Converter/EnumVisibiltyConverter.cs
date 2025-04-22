using System;
using System.Windows.Data;
using System.Windows;

namespace WpfControls.Converter
{
    //Nur wenn eine Enum-Property ein bestimmten Wert hat, ein anders Element sichtbar. Ansonsten ist es Collapsed
    //Visibility="{Binding Path=LengthType, Converter={StaticResource enumVisibiltyConverter}, ConverterParameter=TimerTicks}"
    public class EnumVisibiltyConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }
}
