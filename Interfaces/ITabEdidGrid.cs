using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol.Interfaces
{
    internal interface ITabEdidGrid
    {
        void SetEnterToNextCell(RowColumnIndex? rowColumn = null);
        bool DataGridIsFocused { get; }
    }
}
