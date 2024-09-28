using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfCol
{
    public class CaptionSummaryColumnConverter : IValueConverter
    {
        public static decimal a, b;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var summaryRecordEntry = value as Syncfusion.Data.SummaryRecordEntry;
            if (summaryRecordEntry != null)
            {
                var columnName = parameter.ToString().Contains("1") ? parameter.ToString().Split(' ')[0] : parameter.ToString();
                var summaryRow = summaryRecordEntry.SummaryRow;
                var summaryCol = summaryRow.SummaryColumns.FirstOrDefault(s => s.MappingName == columnName);
                var summaryItems = summaryRecordEntry.SummaryValues;
                if (summaryItems != null && summaryCol != null)
                {
                    var item = summaryItems.FirstOrDefault(s => s.Name == summaryCol.Name);
                    if (item != null)
                    {
                        if (columnName == "Creditor")
                        {
                            if (!parameter.ToString().Contains("1"))
                            {
                                a = decimal.Parse(string.Format("{0:N0}", item.AggregateValues.Values.ToArray()));
                                return $"{string.Format("{0:#,###}", a)}";
                            }
                            else
                            {
                                return $"{string.Format("{0:#,###}", a-b)}";
                            }
                        }
                        if (columnName == "Debtor")
                        {
                            if (!parameter.ToString().Contains("1"))
                            {
                                b = decimal.Parse(string.Format("{0:N0}", item.AggregateValues.Values.ToArray()));
                                return $"{string.Format("{0:#,###}", b)}";
                            }
                            else
                            {
                                return $"{string.Format("{0:#,###}", b-a)}";
                            }
                        }
                    }
                }
            }

            return "Value is wrong";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
