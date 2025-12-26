using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Models
{
    public class Product
    {

        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal PricePerUnitPurchased { get; set; }
     
        public decimal PricePerUnit { get; set; }
        public string Sku { get; set; }

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public int ReoredLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        [ForeignKey("SupplierId")]
        public SuppliersInfromation SuppliersInfromation { get; set; }


    }
}
