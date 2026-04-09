using IMS.COMMON.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Interface.Services
{
    public interface IReportService
    {
        Task<List<WeeklyReportDto>> GetWeeklyReportAsync();
    }
}
