using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Models
{
    public class InventoryBatch
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal PurchasePrice { get; set; }

        public DateTime ReceivedDate { get; set; }

        public bool IsDepleted { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
