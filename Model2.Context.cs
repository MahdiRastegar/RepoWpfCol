﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ColDbEntities1 : DbContext
    {
        public ColDbEntities1()
            : base("name=ColDbEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AcDocument_Header> AcDocument_Header { get; set; }
        public virtual DbSet<Bank> Bank { get; set; }
        public virtual DbSet<ChEvent> ChEvent { get; set; }
        public virtual DbSet<Col> Col { get; set; }
        public virtual DbSet<DocumentType> DocumentType { get; set; }
        public virtual DbSet<Moein> Moein { get; set; }
        public virtual DbSet<PaymentMoneyHeader> PaymentMoneyHeader { get; set; }
        public virtual DbSet<Preferential> Preferential { get; set; }
        public virtual DbSet<RecieveMoneyHeader> RecieveMoneyHeader { get; set; }
        public virtual DbSet<tGroup> tGroup { get; set; }
        public virtual DbSet<CodeSetting> CodeSetting { get; set; }
        public virtual DbSet<AcDocument_Detail> AcDocument_Detail { get; set; }
        public virtual DbSet<CheckPaymentEvent> CheckPaymentEvent { get; set; }
        public virtual DbSet<CheckRecieveEvent> CheckRecieveEvent { get; set; }
        public virtual DbSet<PaymentMoney_Detail> PaymentMoney_Detail { get; set; }
        public virtual DbSet<RecieveMoney_Detail> RecieveMoney_Detail { get; set; }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<CustomerGroup> CustomerGroup { get; set; }
        public virtual DbSet<PriceGroup> PriceGroup { get; set; }
        public virtual DbSet<GroupStorage> GroupStorage { get; set; }
        public virtual DbSet<Storage> Storage { get; set; }
        public virtual DbSet<Unit> Unit { get; set; }
        public virtual DbSet<Commodity> Commodity { get; set; }
        public virtual DbSet<GroupCommodity> GroupCommodity { get; set; }
        public virtual DbSet<CommodityPricingPanel> CommodityPricingPanel { get; set; }
        public virtual DbSet<CodingReceiptTypes> CodingReceiptTypes { get; set; }
        public virtual DbSet<CodingTypesTransfer> CodingTypesTransfer { get; set; }
        public virtual DbSet<StorageReceipt_Detail> StorageReceipt_Detail { get; set; }
        public virtual DbSet<StorageReceiptHeader> StorageReceiptHeader { get; set; }
        public virtual DbSet<StorageTransfer_Detail> StorageTransfer_Detail { get; set; }
        public virtual DbSet<StorageTransferHeader> StorageTransferHeader { get; set; }
        public virtual DbSet<NPStorage_Detail> NPStorage_Detail { get; set; }
        public virtual DbSet<NPStorageHeader> NPStorageHeader { get; set; }
        public virtual DbSet<StorageRotation_Detail> StorageRotation_Detail { get; set; }
        public virtual DbSet<StorageRotationHeader> StorageRotationHeader { get; set; }
        public virtual DbSet<Order_Detail> Order_Detail { get; set; }
        public virtual DbSet<OrderHeader> OrderHeader { get; set; }
        public virtual DbSet<ProductBuy_Detail> ProductBuy_Detail { get; set; }
        public virtual DbSet<ProductBuyHeader> ProductBuyHeader { get; set; }
        public virtual DbSet<AGroup> AGroup { get; set; }
        public virtual DbSet<ProductSell_Detail> ProductSell_Detail { get; set; }
        public virtual DbSet<ProductSellHeader> ProductSellHeader { get; set; }
        public virtual DbSet<PreInvoice_Detail> PreInvoice_Detail { get; set; }
        public virtual DbSet<PreInvoiceHeader> PreInvoiceHeader { get; set; }
    }
}
