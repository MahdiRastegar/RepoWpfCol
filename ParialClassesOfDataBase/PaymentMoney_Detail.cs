using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace WpfCol
{
    public partial class PaymentMoney_Detail
    {
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
        public string GetMoneyType
        {
            get
            {
                switch (MoneyType)
                {
                    case 0:
                        return "نقد";
                    case 1:
                        return "چک";
                    case 2:
                        return "تخفیف";
                    case 3:
                        return "سایر";
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
    }
}
