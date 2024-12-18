using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public class ProductSellViewModel
    {
        public ProductSellViewModel()
        {
            ProductSell_Details = new ObservableCollection<ProductSell_Detail>();
        }
        public ObservableCollection<ProductSell_Detail>  ProductSell_Details { get; set; }
    }
}
