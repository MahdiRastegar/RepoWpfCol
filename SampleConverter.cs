using Syncfusion.UI.Xaml.Grid;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace BcpBindingExtension
{
    public class SampleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var m = value as FrameworkElement;
            if(parameter.ToString()=="vZv")
            {
                //(m as AdvancedFilterControl).FilterTypeComboItems
            }
            if (value != null && m!=null)
            {
                switch(m.Tag.ToString())
                {
                    case "Text Filters":
                        return "فیلتر های متنی";
                    case "Number Filters":
                        return "فیلتر های عددی";
                }
                //return value.ToString() + ", " + ((TextBox)parameter).Text;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && parameter is TextBox)
            {
                string text1 = value as string;
                string textParamter = ((TextBox)parameter).Text;

                return text1.Replace(textParamter, "");

            }

            return value;
        }
    }
}
