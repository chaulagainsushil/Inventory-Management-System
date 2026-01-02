using IMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IMS.APPLICATION.Apllication.Services
{
    public class ReorderPointService
    {
        private readonly ApplicationDbContext _context;

        public ReorderPointService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> CheckReorderPoints()
        {
            var products = await _context.Product
                .Where(p => p.IsActive)
                .ToListAsync();

            var reorderNeeded = new List<object>();

            foreach (var product in products)
            {
                int rop = (int)(product.AverageDailySales * product.LeadTimeDays) + product.SafetyStock;

                if (product.StockQuantity <= rop)
                {
                    reorderNeeded.Add(new
                    {
                        productId = product.Id,
                        productName = product.ProductName,
                        currentStock = product.StockQuantity,
                        reorderPoint = rop,
                        suggestedOrderQty = rop + product.SafetyStock
                    });
                }
            }

            return reorderNeeded;
        }
    }
}
