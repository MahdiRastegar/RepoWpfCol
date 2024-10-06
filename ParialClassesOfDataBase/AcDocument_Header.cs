using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public partial class AcDocument_Header
    {
        public void RefreshSumColumns()
        {
            _SumCreditor = _SumDebtor = null;
        }
        Nullable<decimal> _SumDebtor;
        public Nullable<decimal> SumDebtor
        {
            get
            {
                if (_SumDebtor == null)
                    _SumDebtor = AcDocument_Detail.Sum(x => x.Debtor);
                return _SumDebtor;
            }
            set
            {
                _SumDebtor = value;
            }
        }
        Nullable<decimal> _SumCreditor;
        public Nullable<decimal> SumCreditor
        {
            get
            {
                if (_SumCreditor == null)
                    _SumCreditor = AcDocument_Detail.Sum(x => x.Creditor);
                return _SumCreditor;
            }
            set
            {
                _SumCreditor = value;
            }
        }
        public Nullable<decimal> Difference
        {
            get
            {
                return SumDebtor - SumCreditor;
            }
        }
    }
}
