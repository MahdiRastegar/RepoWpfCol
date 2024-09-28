using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCol;

namespace WpfCol
{
    public class AcDocumentViewModel
    {
        public AcDocumentViewModel()
        {
            acDocument_Details = new ObservableCollection<AcDocument_Detail>();
        }
        public ObservableCollection<AcDocument_Detail> acDocument_Details {  get; set; }
    }
}
