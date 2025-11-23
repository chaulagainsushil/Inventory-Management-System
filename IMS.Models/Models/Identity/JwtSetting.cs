using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Models.Identity
{
    public class JwtSetting
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }
        public String SecretKey { get; set; }


    }
}
