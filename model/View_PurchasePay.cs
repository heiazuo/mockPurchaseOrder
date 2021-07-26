namespace MopckPurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class View_PurchasePay
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PurchaseStatementId { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "money")]
        public decimal ChargeOff { get; set; }

        [Key]
        [Column(Order = 3, TypeName = "money")]
        public decimal PayMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal? DiscountMoney { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(50)]
        public string PayType { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime PayDate { get; set; }

        [StringLength(200)]
        public string Memo { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 7)]
        public DateTime UpdateTime { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BranchId { get; set; }

        [Key]
        [Column(Order = 9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SupplierId { get; set; }

        [Key]
        [Column(Order = 10, TypeName = "money")]
        public decimal SumMoney { get; set; }

        [Key]
        [Column(Order = 11, TypeName = "money")]
        public decimal PaidMoney { get; set; }

        [Key]
        [Column(Order = 12)]
        [StringLength(50)]
        public string PayStatus { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 13)]
        [StringLength(100)]
        public string ShortName { get; set; }

        [StringLength(100)]
        public string TelPhone { get; set; }

        [StringLength(50)]
        public string LinkMan { get; set; }

        [StringLength(100)]
        public string Mobile { get; set; }

        [Key]
        [Column(Order = 14)]
        [StringLength(50)]
        public string UserName { get; set; }

        [StringLength(50)]
        public string ApplyName { get; set; }

        public int? ApplyUserId { get; set; }

        [StringLength(300)]
        public string PayAccount { get; set; }

        public int? PayAccountId { get; set; }
    }
}
