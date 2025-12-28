using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.COMMON.Dtos
{
    public class ProductCreateDto
    {
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal PricePerUnit { get; set; }
        public string Sku { get; set; }
        public decimal PricePerUnitPurchased { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
