using System;
using System.Globalization;
using System.Windows.Data;

namespace SourceCodeIndexer.UI
{
    public class BooleanToGridHeightConverter : IValueConverter
    {
        /// <summary>
        /// Converts bool value of type bool to Height
        /// </summary>
        /// <returns>Height zero or asterik</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? valueResult = value as bool?;
            return valueResult.HasValue
                ? valueResult.Value ? "2*" : "0"
                : "0";
        }

        /// <summary>
        /// no convert back return null
        /// </summary>
        /// <returns>null</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
