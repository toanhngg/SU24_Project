using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class TableOrder
    {
        public int? TableId { get; set; }
        public int? OrderId { get; set; }

        public virtual Order? Order { get; set; }
        public virtual Table? Table { get; set; }
    }
}
