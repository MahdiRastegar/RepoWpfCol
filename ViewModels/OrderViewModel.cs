using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            Order_Details = new ObservableCollection<Order_Detail>();
        }
        public ObservableCollection<Order_Detail>  Order_Details { get; set; }
    }
}
