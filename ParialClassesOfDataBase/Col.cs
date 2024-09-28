using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCol
{
    public partial class Col
    {
        public string GetType2
        {
            get
            {
                switch (Type)
                {
                    case 0:
                        return "بدهکار";
                    case 1:
                        return "بستانکار";
                    case 2:
                        return "هردو";
                }
                return "";
            }
        }
        public string GetAction
        {
            get
            {
                switch (Action)
                {
                    case 0:
                        return "ترازنامه ای";
                    case 1:
                        return "سود و زیانی";
                }
                return "";
            }
        }
    }
}
