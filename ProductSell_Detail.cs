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
    
    public partial class ProductSell_Detail
    {
        public System.Guid Id { get; set; }
        public System.Guid fk_CommodityId { get; set; }
        public decimal Value { get; set; }
        public System.Guid fk_HeaderId { get; set; }
        public int Indexer { get; set; }
        public decimal Fee { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxPercent { get; set; }
    
        public virtual Commodity Commodity { get; set; }
        public virtual ProductSellHeader ProductSellHeader { get; set; }
    }
}