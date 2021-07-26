namespace MopckPurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class I_Analysis
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        [StringLength(3000)]
        public string AppKey { get; set; }

        [StringLength(3000)]
        public string AppSecret { get; set; }

        [StringLength(500)]
        public string Token { get; set; }

        public DateTime? ExpiresTime { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
