using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCol;

namespace WpfCol
{
    public class RecieveMoneyViewModel
    {
        public RecieveMoneyViewModel()
        {
            Banks = new ObservableCollection<Bank>();
            recieveMoney_Details = new ObservableCollection<RecieveMoney_Detail>();
        }
        public ObservableCollection<Bank> Banks {  get; set; }
        public ObservableCollection<RecieveMoney_Detail> recieveMoney_Details {  get; set; }
    }
}
