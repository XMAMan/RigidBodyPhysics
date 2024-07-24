using System;
using System.Windows.Data;
using System.Windows;

namespace WpfControls.View
{
    //Hiermit kann man RadioButtons zur Auwahl von ein Enum-Wert verwenden:
    //public enum AnimationLengthType
    //{
    //    TimerTicks, 
    //    Seconds    
    //}
    //<DockPanel.Resources>
    //    <local:EnumBooleanConverter x:Key="enumBooleanConverter" />
    //</DockPanel.Resources>
    //<RadioButton VerticalAlignment = "Center" IsChecked="{Binding Path=LengthType, Converter={StaticResource enumBooleanConverter}, ConverterParameter=TimerTicks}" Content="TimerTicks"/>
    //<RadioButton VerticalAlignment = "Center" IsChecked="{Binding Path=LengthType, Converter={StaticResource enumBooleanConverter}, ConverterParameter=Seconds}" Content="Seconds"/>
    public class EnumBooleanConverter : IValueConverter
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

            return parameterValue.Equals(value);
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
