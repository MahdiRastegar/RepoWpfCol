using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class PreInvoiceViewModel
    {
        public PreInvoiceViewModel()
        {
            PreInvoice_Details = new ObservableCollection<PreInvoice_Detail>();
        }
        public ObservableCollection<PreInvoice_Detail>  PreInvoice_Details { get; set; }
    }
}
