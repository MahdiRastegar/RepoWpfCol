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
    
    public partial class Moein
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Moein()
        {
            this.AcDocument_Detail = new HashSet<AcDocument_Detail>();
            this.PaymentMoney_Detail = new HashSet<PaymentMoney_Detail>();
            this.PaymentMoneyHeader = new HashSet<PaymentMoneyHeader>();
            this.RecieveMoney_Detail = new HashSet<RecieveMoney_Detail>();
            this.RecieveMoneyHeader = new HashSet<RecieveMoneyHeader>();
        }
    
        public System.Guid Id { get; set; }
        public int MoeinCode { get; set; }
        public System.Guid fk_ColId { get; set; }
        public string MoeinName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AcDocument_Detail> AcDocument_Detail { get; set; }
        public virtual Col Col { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentMoney_Detail> PaymentMoney_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentMoneyHeader> PaymentMoneyHeader { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecieveMoney_Detail> RecieveMoney_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecieveMoneyHeader> RecieveMoneyHeader { get; set; }
    }
}
