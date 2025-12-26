using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TextAnalyzer
{
    public class TextToVisibilityConverter : IValueConverter
    {
        //один обший экземпляр для показа плейсхолдера
        public static TextToVisibilityConverter Instance = new TextToVisibilityConverter();

        //проверка показывать плейсхолдер или нет
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        //не вызываемый метод для обратной логики плейсхолдера
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}