using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class StorageRotationViewModel
    {
        public StorageRotationViewModel()
        {
            StorageRotation_Details = new ObservableCollection<StorageRotation_Detail>();
        }
        public ObservableCollection<StorageRotation_Detail>  StorageRotation_Details { get; set; }
    }
}
