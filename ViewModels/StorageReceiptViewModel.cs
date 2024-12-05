using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class StorageReceiptViewModel
    {
        public StorageReceiptViewModel()
        {
            StorageReceipt_Details = new ObservableCollection<StorageReceipt_Detail>();
        }
        public ObservableCollection<StorageReceipt_Detail>  StorageReceipt_Details { get; set; }
    }
}
