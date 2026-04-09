using IMS.COMMON.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Interface.Repository
{
    public  interface IReportRepository
    {
        Task<List<WeeklyReportDto>> GetWeeklyReportAsync();
    }
}
