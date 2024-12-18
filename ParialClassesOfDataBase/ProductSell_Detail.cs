using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace WpfCol
{
    public partial class ProductSell_Detail : IDataErrorInfo
    {

        public decimal SumNextDiscount
        {
            get
            {
                return (Value * Fee) - Discount;
            }
        }
        public decimal Tax
        {
            get
            {
                if (Commodity?.Taxable == true)
                    return SumNextDiscount * 10 / 100;
                else
                    return 0;
            }
        }
        public decimal Sum
        {
            get
            {
                return SumNextDiscount + Tax;
            }
        }
        [Display(AutoGenerateField = false)]
        public string Error
        {
            get
            {
                if (_Errors.Count > 0)
                    return "اطلاعات در گرید به شکل درست وارد نشده";
                return string.Empty;
            }
        }
        private List<string> _Errors = new List<string>();
        public void ClearErrors()
        {
            _Errors.Clear();
        }
        public string this[string columnName]
        {
            get
            {
                if (Commodity == null)
                    return string.Empty;
                switch (columnName)
                {
                    case "Value":
                        if (Value==0)
                        {
                            _Errors.AddUniqueItem("Value");
                            return "مقدار را وارد کنید!";
                        }
                        _Errors.Remove("Value");
                        return string.Empty;
                    case "Fee":
                        if (Fee==0)
                        {
                            _Errors.AddUniqueItem("Fee");
                            return "فی را وارد کنید!";
                        }
                        _Errors.Remove("Fee");
                        return string.Empty;
                }

                return string.Empty;
            }
        }
    }
}
