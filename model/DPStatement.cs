namespace MopckPurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DPStatement")]
    public partial class DPStatement
    {
        public int Id { get; set; }

        public int BranchId { get; set; }

        public int SupplierId { get; set; }

        [Column(TypeName = "money")]
        public decimal SumMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal PaidMoney { get; set; }

        [Required]
        [StringLength(50)]
        public string PayStatus { get; set; }

        [Column(TypeName = "money")]
        public decimal ChargeOff { get; set; }

        [StringLength(500)]
        public string Memo { get; set; }

        public int UpdateBy { get; set; }

        public DateTime UpdateAt { get; set; }

        [Column(TypeName = "money")]
        public decimal InvoiceMoney { get; set; }

        [StringLength(50)]
        public string InvoiceStatus { get; set; }

        [Column(TypeName = "money")]
        public decimal PaidApplyMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal InvoiceApplyMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal InvoiceChargeOff { get; set; }
    }
}
