using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public partial class AcDocument_Detail
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
        //_PreferentialName = $"{Preferential.PreferentialName}-{Account.Moein.MoeinName}";
        public string Debtor2
        {
            get 
            {
                if (Debtor == null)
                    return null;
                return Debtor.ToComma();
            }
            set { }
        }
        public string Creditor2
        {
            get 
            {
                if (Creditor == null)
                    return null;
                return Creditor.ToComma();
            }
            set { }
        }
    }
}
