namespace MopckPurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DistributionReceiveMoney")]
    public partial class DistributionReceiveMoney
    {
        public int Id { get; set; }

        public int BranchId { get; set; }

        public int PBranchId { get; set; }

        [Column(TypeName = "money")]
        public decimal ChargeOff { get; set; }

        [Column(TypeName = "money")]
        public decimal ReceiveMoney { get; set; }

        public DateTime ReceiveDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PayType { get; set; }

        public int? ReceiveAccount { get; set; }

        [StringLength(500)]
        public string Memo { get; set; }

        public int UserId { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsDelete { get; set; }
    }
}
