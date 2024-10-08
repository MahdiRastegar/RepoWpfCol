using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCol;

namespace WpfCol
{
    public class PaymentMoneyViewModel
    {
        public PaymentMoneyViewModel()
        {
            Banks = new ObservableCollection<Bank>();
            paymentMoney_Details = new ObservableCollection<PaymentMoney_Detail>();
        }
        public ObservableCollection<Bank> Banks {  get; set; }
        public ObservableCollection<PaymentMoney_Detail> paymentMoney_Details {  get; set; }
    }
}
