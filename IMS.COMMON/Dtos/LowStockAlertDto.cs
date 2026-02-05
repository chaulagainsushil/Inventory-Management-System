using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.COMMON.Dtos
{
    public class LowStockAlertDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int CurrentStock { get; set; }
        public int ReorderPoint { get; set; }
        public decimal AverageDailySales { get; set; }
        public int LeadTimeDays { get; set; }
        public int SafetyStock { get; set; }
        public string UrgencyLevel { get; set; } = null!;
    }
}
