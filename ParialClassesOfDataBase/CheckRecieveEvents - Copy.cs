using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public partial class CheckPaymentEvent
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
    }
}
