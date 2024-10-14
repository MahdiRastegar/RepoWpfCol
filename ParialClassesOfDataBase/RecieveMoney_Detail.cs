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
    public partial class RecieveMoney_Detail:IDataErrorInfo
    {
        public RecieveMoney_Detail()
        {
            MoneyType = 255;
        }
        private string _Name;
        public string Name
        {
            get
            {
                if (Preferential == null || Moein == null)
                {
                    _Name = null;
                    return _Name;
                }
                _Name = $"{Preferential.PreferentialName}-{Moein.MoeinName}";
                return _Name;
            }
            set { _Name = value; }
        }
        private string _ColeMoein;
        public string ColeMoein
        {
            get
            {
                if (Moein == null)
                {
                    _ColeMoein = null;
                    return _ColeMoein;
                }
                _ColeMoein = $"{Moein.Col.ColCode}{Moein.MoeinCode}";
                return _ColeMoein;
            }
            set { _ColeMoein = value; }
        }
        private string _PreferentialCode;
        public string PreferentialCode
        {
            get
            {
                if (Preferential == null)
                {
                    _PreferentialCode = null;
                    return _PreferentialCode;
                }
                _PreferentialCode = $"{Preferential.PreferentialCode}";
                return _PreferentialCode;
            }
            set { _PreferentialCode = value; }
        }

        public string DateString
        {
            get 
            {
                if (Date != null)
                {
                    return Date.Value.ToPersianDateString();
                }
                return null; 
            }
            set
            {
                if (value.Count(h => h == '/') == 2)
                {
                    try
                    {
                        Date = value.ToDateTimeOfString();
                    }
                    catch { }
                }
            }
        }

        public string GetMoneyType
        {
            get
            {
                switch (MoneyType)
                {
                    case 0:
                        return "1-نقد";
                    case 1:
                        return "2-چک";
                    case 2:
                        return "3-تخفیف";
                    case 3:
                        return "4-سایر";
                }
                return null;
            }
            set { }                
        }
        public string Price2
        {
            get
            {
                if (Price == 0)
                    return null;
                return (Price as decimal?).ToComma();
            }
            set { }
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
                if (MoneyType == 255)
                    return string.Empty;
                switch (columnName)
                {
                    case "ColeMoein":
                        if (ColeMoein == "" || ColeMoein == null)
                        {
                            _Errors.AddUniqueItem("ColeMoein");
                            return "کد کل و معین را وارد کنید!";
                        }
                        _Errors.Remove("ColeMoein");
                        return string.Empty;
                    case "PreferentialCode":
                        if (PreferentialCode == "" || PreferentialCode == null)
                        {
                            _Errors.AddUniqueItem("PreferentialCode");
                            return "کد تفضیلی را وارد کنید!";
                        }
                        _Errors.Remove("PreferentialCode");
                        return string.Empty;
                }
                if (columnName.Equals("Price"))
                {
                    if (Price == 0)
                    {
                        _Errors.AddUniqueItem("Price");
                        return "مبلغ نمی تواند صفر باشد!";
                    }
                    _Errors.Remove("Price");
                    return string.Empty;
                }
                switch (MoneyType)
                {
                    case 0:
                        break;
                    case 1:
                        switch (columnName)
                        {
                            case "DateString":
                                if (Date == null)
                                {
                                    _Errors.AddUniqueItem("DateString");
                                    return "تاریخ را وارد کنید!";
                                }
                                _Errors.Remove("DateString");
                                return string.Empty;
                            case "Bank":
                                if (Bank == null)
                                {
                                    _Errors.AddUniqueItem("Bank");
                                    return "نام بانک را وارد کنید!";
                                }
                                _Errors.Remove("Bank");
                                return string.Empty;
                            case "Number":
                                _Errors.Remove("Number3");
                                if (Number == "" || Number == null)
                                {
                                    _Errors.AddUniqueItem("Number1");
                                    return "شماره چک را وارد کنید!";
                                }
                                _Errors.Remove("Number1");
                                return string.Empty;
                        }
                        break;
                    case 2:
                        break;
                    case 3:
                        _Errors.Remove("Number1");
                        if (columnName == "Number" && (Number == "" || Number == null))
                        {
                            _Errors.AddUniqueItem("Number3");
                            return "شماره را وارد کنید!";
                        }
                        _Errors.Remove("Number3");
                        return string.Empty;
                }

                return string.Empty;
            }
        }
    }
}
