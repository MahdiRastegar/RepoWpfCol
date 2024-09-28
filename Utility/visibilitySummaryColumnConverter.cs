using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfCol
{
    public class VisibilitySummaryColumnConverter : IValueConverter
    {
        static decimal a, b;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var columnName = parameter.ToString();

            if (columnName == "Creditor"&& CaptionSummaryColumnConverter.a > CaptionSummaryColumnConverter.b)
            {
                return Visibility.Visible;
            }
            if (columnName == "Debtor" && CaptionSummaryColumnConverter.b > CaptionSummaryColumnConverter.a)
            {
                return Visibility.Visible;

            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
