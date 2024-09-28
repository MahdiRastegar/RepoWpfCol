using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfCol
{
    public class PersianDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                PersianCalendar persianCalendar = new PersianCalendar();
                return $"{persianCalendar.GetYear(dateTime)}/{persianCalendar.GetMonth(dateTime)}/{persianCalendar.GetDayOfMonth(dateTime)}";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
