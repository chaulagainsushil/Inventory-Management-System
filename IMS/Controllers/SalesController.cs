using IMS.COMMON.Dtos;
using IMS.Data;
using IMS.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- Add this using directive

namespace IMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale(SaleCreateDto dto)
        {
            // Begin database transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                decimal subTotal = 0;
                var saleItems = new List<SaleItem>();

                foreach (var item in dto.Items)
                {
                    var product = await _context.Product
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                        return BadRequest($"Product not found: {item.ProductId}");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"Insufficient stock for {product.ProductName}");

                    // Stock deduction
                    product.StockQuantity -= item.Quantity;

                    decimal totalPrice = product.PricePerUnit * item.Quantity;
                    subTotal += totalPrice;

                    saleItems.Add(new SaleItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.PricePerUnit,
                        TotalPrice = totalPrice
                    });
                }

                var sale = new Sale
                {
                    InvoiceNo = $"INV-{DateTime.Now.Ticks}",
                    SaleDate = DateTime.UtcNow,
                    CustomerId = dto.CustomerId,
                    UserId = dto.UserId,
                    SubTotal = subTotal,
                    Discount = dto.Discount,
                    Tax = dto.Tax,
                    TotalAmount = subTotal - dto.Discount + dto.Tax,
                    PaymentMethod = dto.PaymentMethod,
                    SaleItems = saleItems
                };

                _context.Sale.Add(sale);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Sale completed successfully",
                    invoiceNo = sale.InvoiceNo,
                    total = sale.TotalAmount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("report/daily")]
        public async Task<IActionResult> DailySalesReport(DateTime date)
        {
            var report = await _context.Sale
                .Where(s => s.SaleDate.Date == date.Date)
                .Select(s => new
                {
                    s.InvoiceNo,
                    s.TotalAmount,
                    s.PaymentMethod,
                    s.SaleDate
                })
                .ToListAsync();

            return Ok(report);
        }
        [HttpGet("report/range")]
        public async Task<IActionResult> SalesReportByDateRange(
        DateTime fromDate,
        DateTime toDate)
        {
            var report = await _context.Sale
                .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate)
                .Select(s => new
                {
                    s.InvoiceNo,
                    s.TotalAmount,
                    s.SaleDate
                })
                .ToListAsync();

            return Ok(report);
        }

        [HttpGet("report/summary")]
        public async Task<IActionResult> SalesSummary()
        {
            var totalSales = await _context.Sale.SumAsync(s => s.TotalAmount);
            var totalInvoices = await _context.Sale.CountAsync();

            return Ok(new
            {
                totalSales,
                totalInvoices
            });
        }

        [HttpGet("monthly-revenue")]
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var totalRevenue = await _context.Sale
                .Where(s => s.SaleDate.Month == currentMonth &&
                            s.SaleDate.Year == currentYear)
                .SumAsync(s => s.TotalAmount);

            return Ok(new
            {
                
                TotalRevenue = totalRevenue
            });
        }



    }
}
