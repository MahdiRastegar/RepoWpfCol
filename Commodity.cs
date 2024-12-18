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
    
    public partial class Commodity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Commodity()
        {
            this.CommodityPricingPanel = new HashSet<CommodityPricingPanel>();
            this.StorageReceipt_Detail = new HashSet<StorageReceipt_Detail>();
            this.StorageTransfer_Detail = new HashSet<StorageTransfer_Detail>();
            this.NPStorage_Detail = new HashSet<NPStorage_Detail>();
            this.StorageRotation_Detail = new HashSet<StorageRotation_Detail>();
            this.Order_Detail = new HashSet<Order_Detail>();
            this.ProductBuy_Detail = new HashSet<ProductBuy_Detail>();
            this.ProductSell_Detail = new HashSet<ProductSell_Detail>();
            this.PreInvoice_Detail = new HashSet<PreInvoice_Detail>();
        }
    
        public System.Guid Id { get; set; }
        public int Code { get; set; }
        public System.Guid fk_GroupId { get; set; }
        public string Name { get; set; }
        public System.Guid fk_UnitId { get; set; }
        public Nullable<bool> Taxable { get; set; }
    
        public virtual GroupCommodity GroupCommodity { get; set; }
        public virtual Unit Unit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CommodityPricingPanel> CommodityPricingPanel { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StorageReceipt_Detail> StorageReceipt_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StorageTransfer_Detail> StorageTransfer_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NPStorage_Detail> NPStorage_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StorageRotation_Detail> StorageRotation_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order_Detail> Order_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductBuy_Detail> ProductBuy_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductSell_Detail> ProductSell_Detail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PreInvoice_Detail> PreInvoice_Detail { get; set; }
    }
}
