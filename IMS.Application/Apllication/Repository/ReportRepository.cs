using IMS.APPLICATION.Interface.Repository;
using IMS.COMMON.Dtos;
using IMS.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Apllication.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _db;

        public ReportRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<WeeklyReportDto>> GetWeeklyReportAsync()
        {
            var today = DateTime.UtcNow.Date;
            var result = new List<WeeklyReportDto>();

            for (int i = 0; i >= -3; i--) // current week + last 3 weeks
            {
                var weekStart = today.AddDays(i * 7 - (int)today.DayOfWeek + 1); // Monday
                var weekEnd = weekStart.AddDays(6); // Sunday

                var productIn = await _db.Product
                    .Where(p => p.CreatedAt.Date >= weekStart && p.CreatedAt.Date <= weekEnd)
                    .CountAsync();

                var productOut = await _db.SaleItem
                    .Where(s => s.Sale.SaleDate.Date >= weekStart && s.Sale.SaleDate.Date <= weekEnd)
                    .SumAsync(s => (int?)s.Quantity) ?? 0;

                result.Add(new WeeklyReportDto
                {
                    Year = weekStart.Year,
                    WeekNumber = ISOWeek.GetWeekOfYear(weekStart),
                    ProductIn = productIn,
                    ProductOut = productOut
                });
            }

            return result.OrderByDescending(r => r.Year)
                         .ThenByDescending(r => r.WeekNumber)
                         .ToList();
        }
    }
}