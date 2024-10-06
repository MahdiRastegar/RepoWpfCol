using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCol.Interfaces;

namespace WpfCol
{
    public class AccountSearchClass:ISearch
    {
        public Guid Id { get; set; }
        public string Moein { get; set; }
        public string MoeinName { get; set;}
        public string ColMoein { get; set;}
        public string Result 
        { 
            get => ColMoein; set => throw new NotImplementedException(); 
        }
    }
}
