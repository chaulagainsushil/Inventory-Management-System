using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.COMMON.Dtos
{
    public class SaleCreateDto
    {
        public int? CustomerId { get; set; }
        public string UserId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }

        public List<SaleItemCreateDto> Items { get; set; }
    }
}
