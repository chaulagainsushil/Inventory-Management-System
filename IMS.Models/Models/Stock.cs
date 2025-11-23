using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Models
{
    public class Stock
    {
        public int StockId { get; set; }
        public int ProductId { get; set; }
        public int? WarehouseId { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ReservedQuantity { get; set; }
        public DateTime LastUpdated { get; set; }

        // Navigation properties
        public Product Product { get; set; }
        public WareHouse WareHouse { get; set; }
    }
}
