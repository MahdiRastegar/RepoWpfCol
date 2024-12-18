using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class NPStorageViewModel
    {
        public NPStorageViewModel()
        {
            NPStorage_Details = new ObservableCollection<NPStorage_Detail>();
        }
        public ObservableCollection<NPStorage_Detail>  NPStorage_Details { get; set; }
    }
}
