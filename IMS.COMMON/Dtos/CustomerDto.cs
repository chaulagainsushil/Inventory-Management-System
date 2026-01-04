using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.COMMON.Dtos
{
    public class CustomerDto
    {
        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
