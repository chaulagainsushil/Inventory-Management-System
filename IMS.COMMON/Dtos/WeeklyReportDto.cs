using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.COMMON.Dtos
{
    public class WeeklyReportDto
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public int ProductIn { get; set; }
        public int ProductOut { get; set; }
    }
}
