using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Syncfusion.UI.Xaml.Grid;

namespace WpfCol
{
    public class MyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TemplateA { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FilterElement)
            {
                return TemplateA;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
