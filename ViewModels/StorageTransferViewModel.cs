using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class StorageTransferViewModel
    {
        public StorageTransferViewModel()
        {
            StorageTransfer_Details = new ObservableCollection<StorageTransfer_Detail>();
        }
        public ObservableCollection<StorageTransfer_Detail>  StorageTransfer_Details { get; set; }
    }
}
