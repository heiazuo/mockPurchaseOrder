namespace MopckPurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MemberOrderDept")]
    public partial class MemberOrderDept
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int MemberId { get; set; }

        public int DeptId { get; set; }

        [Required]
        [StringLength(100)]
        public string DeptName { get; set; }

        [StringLength(100)]
        public string Code { get; set; }

        [StringLength(100)]
        public string PCode { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
