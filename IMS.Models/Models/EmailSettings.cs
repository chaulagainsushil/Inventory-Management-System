using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Models.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = null!;
        public int Port { get; set; }
        public string SenderName { get; set; } = null!;
        public string SenderEmail { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool EnableSSL { get; set; }
        public string AdminEmail { get; set; } = null!;
    }
}
