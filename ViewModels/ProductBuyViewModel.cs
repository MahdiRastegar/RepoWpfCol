using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class ProductBuyViewModel
    {
        public ProductBuyViewModel()
        {
            ProductBuy_Details = new ObservableCollection<ProductBuy_Detail>();
        }
        public ObservableCollection<ProductBuy_Detail>  ProductBuy_Details { get; set; }
    }
}
