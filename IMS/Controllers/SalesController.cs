using IMS.APPLICATION.Apllication.Services;
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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                decimal subTotal = 0;
                var saleItems = new List<SaleItem>();
                var lowStockProducts = new List<object>();

                foreach (var item in dto.Items)
                {
                    var product = await _context.Product
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                        return BadRequest($"Product not found: {item.ProductId}");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"Insufficient stock for {product.ProductName}");

                    // Update stock
                    product.StockQuantity -= item.Quantity;

                    // ✅ CHECK REORDER POINT
                    int calculatedROP = (int)(product.AverageDailySales * product.LeadTimeDays) + product.SafetyStock;

                    if (product.StockQuantity <= calculatedROP)
                    {
                        lowStockProducts.Add(new
                        {
                            productName = product.ProductName,
                            currentStock = product.StockQuantity,
                            reorderPoint = calculatedROP,
                            reorderLevel = product.ReorderLevel,
                            suggestedOrderQty = calculatedROP + product.SafetyStock - product.StockQuantity
                        });
                    }

                    // Update average daily sales (simple moving average)
                    product.AverageDailySales = await CalculateAverageDailySales(product.Id);

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
                    total = sale.TotalAmount,
                    reorderAlerts = lowStockProducts
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        // Helper method to calculate average daily sales
        private async Task<decimal> CalculateAverageDailySales(int productId)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var totalSold = await _context.SaleItem
                .Where(si => si.ProductId == productId &&
                             si.Sale.SaleDate >= thirtyDaysAgo)
                .SumAsync(si => si.Quantity);

            return totalSold / 30m;
        }
        [HttpGet("monthly-total-sale")]
        public async Task<IActionResult> GetCurrentMonthTotalSale()
        {
            var now = DateTime.UtcNow;

            var totalSale = await _context.Sale
                .Where(s =>
                    s.SaleDate.Year == now.Year &&
                    s.SaleDate.Month == now.Month &&
                    s.Status == "Completed")
                .SumAsync(s => s.TotalAmount);

            return Ok(totalSale);
        }


    }
}