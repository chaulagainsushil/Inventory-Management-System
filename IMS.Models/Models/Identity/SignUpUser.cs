using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Models.Identity
{
    public class SignUpUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public String Name { get; set; }
        [Required]
        public String Email { get; set; }
        [Required]
        [MinLength(8)]

        public String Password { get; set; }
        [Required]
        public String ConfirmPassword { get; set; }
        [Required]
        public String PhoneNumber { get; set; }

    }
}
