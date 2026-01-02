using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.COMMON.Dtos
{
    public class SaleCreateDto
    {
        public int? CustomerId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public List<SaleItemCreateDto> Items { get; set; }

        public decimal Discount { get; set; }
        public decimal Tax { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}
