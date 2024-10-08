//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WpfCol
{
    using System;
    using System.Collections.Generic;
    
    public partial class PaymentMoney_Detail
    {
        public System.Guid Id { get; set; }
        public System.Guid fkHeaderId { get; set; }
        public decimal Price { get; set; }
        public string BranchName { get; set; }
        public string Number { get; set; }
        public Nullable<System.Guid> fkBank { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public byte MoneyType { get; set; }
        public System.Guid fk_PreferentialId { get; set; }
        public System.Guid fk_MoeinId { get; set; }
    
        public virtual Bank Bank { get; set; }
        public virtual Moein Moein { get; set; }
        public virtual PaymentMoneyHeader PaymentMoneyHeader { get; set; }
        public virtual Preferential Preferential { get; set; }
    }
}