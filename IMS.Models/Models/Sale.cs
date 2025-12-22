using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNo { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }

        public int? CustomerId { get; set; }   // Optional

        [Required]
        public string UserId { get; set; }     // Cashier (AspNetUsers)

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(20)]
        public string PaymentMethod { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Completed";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<SaleItem> SaleItems { get; set; }
    }
}
