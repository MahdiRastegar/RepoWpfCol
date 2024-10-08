using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public partial class PaymentMoneyHeader
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
    }
}
