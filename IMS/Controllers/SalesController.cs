using IMS.APPLICATION.Apllication.Services;
using IMS.COMMON.Dtos;
using IMS.Data;
using IMS.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                    // Update average daily sales BEFORE ROP calculation
                    // (so we use the most recent data)
                    product.AverageDailySales = await CalculateAverageDailySales(product.Id);

                    // ✅ DYNAMIC ROP CALCULATION
                    int calculatedROP = (int)(product.AverageDailySales * product.LeadTimeDays)
                                        + product.SafetyStock;

                    // Check if reorder needed
                    if (product.StockQuantity <= calculatedROP)
                    {
                        // Calculate suggested order quantity intelligently
                        int suggestedQty = Math.Max(
                            calculatedROP + product.SafetyStock - product.StockQuantity,
                            (int)(product.AverageDailySales * 7) // At least 1 week supply
                        );

                        lowStockProducts.Add(new
                        {
                            productId = product.Id,
                            productName = product.ProductName,
                            currentStock = product.StockQuantity,
                            reorderPoint = calculatedROP,
                            averageDailySales = product.AverageDailySales,
                            leadTimeDays = product.LeadTimeDays,
                            safetyStock = product.SafetyStock,
                            suggestedOrderQty = suggestedQty,
                            urgencyLevel = GetUrgencyLevel(product.StockQuantity, calculatedROP, product.SafetyStock)
                        });
                    }

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
                    reorderAlerts = lowStockProducts,
                    alertCount = lowStockProducts.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculate average daily sales based on last 30 days
        /// </summary>
        private async Task<decimal> CalculateAverageDailySales(int productId)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var totalSold = await _context.SaleItem
                .Include(si => si.Sale)
                .Where(si => si.ProductId == productId &&
                             si.Sale.SaleDate >= thirtyDaysAgo &&
                             si.Sale.Status == "Completed")
                .SumAsync(si => si.Quantity);

            return totalSold / 30m;
        }

        /// <summary>
        /// Determine urgency level for reordering
        /// </summary>
        private string GetUrgencyLevel(int currentStock, int rop, int safetyStock)
        {
            if (currentStock <= safetyStock)
                return "CRITICAL"; // At or below safety stock
            else if (currentStock <= rop * 0.5)
                return "HIGH"; // Less than 50% of ROP
            else if (currentStock <= rop * 0.75)
                return "MEDIUM"; // Less than 75% of ROP
            else
                return "LOW"; // Just below ROP
        }

        /// <summary>
        /// Get all products that need reordering
        /// </summary>
        [HttpGet("reorder-alerts")]
        public async Task<IActionResult> GetReorderAlerts()
        {
            var products = await _context.Product
                .Where(p => p.IsActive)
                .ToListAsync();

            var alerts = new List<object>();

            foreach (var product in products)
            {
                int calculatedROP = (int)(product.AverageDailySales * product.LeadTimeDays)
                                    + product.SafetyStock;

                if (product.StockQuantity <= calculatedROP)
                {
                    int suggestedQty = Math.Max(
                        calculatedROP + product.SafetyStock - product.StockQuantity,
                        (int)(product.AverageDailySales * 7)
                    );

                    alerts.Add(new
                    {
                        productId = product.Id,
                        productName = product.ProductName,
                        currentStock = product.StockQuantity,
                        reorderPoint = calculatedROP,
                        averageDailySales = product.AverageDailySales,
                        leadTimeDays = product.LeadTimeDays,
                        safetyStock = product.SafetyStock,
                        suggestedOrderQty = suggestedQty,
                        urgencyLevel = GetUrgencyLevel(product.StockQuantity, calculatedROP, product.SafetyStock)
                    });
                }
            }

            return Ok(new
            {
                totalAlerts = alerts.Count,
                alerts = alerts.OrderBy(a => ((dynamic)a).urgencyLevel == "CRITICAL" ? 0 :
                                             ((dynamic)a).urgencyLevel == "HIGH" ? 1 :
                                             ((dynamic)a).urgencyLevel == "MEDIUM" ? 2 : 3)
            });
        }

        /// <summary>
        /// Get current month total sales
        /// </summary>
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

            return Ok(new { totalSales = totalSale, month = now.ToString("MMMM yyyy") });
        }

        /// <summary>
        /// Get ROP analytics for a specific product
        /// </summary>
        [HttpGet("rop-analytics/{productId}")]
        public async Task<IActionResult> GetROPAnalytics(int productId)
        {
            var product = await _context.Product
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound("Product not found");

            int calculatedROP = (int)(product.AverageDailySales * product.LeadTimeDays)
                                + product.SafetyStock;

            // Days until stockout
            decimal daysUntilStockout = product.AverageDailySales > 0
                ? product.StockQuantity / product.AverageDailySales
                : 0;

            return Ok(new
            {
                productName = product.ProductName,
                currentStock = product.StockQuantity,
                averageDailySales = product.AverageDailySales,
                leadTimeDays = product.LeadTimeDays,
                safetyStock = product.SafetyStock,
                calculatedROP = calculatedROP,
                daysUntilStockout = Math.Round(daysUntilStockout, 1),
                shouldReorder = product.StockQuantity <= calculatedROP,
                urgencyLevel = GetUrgencyLevel(product.StockQuantity, calculatedROP, product.SafetyStock)
            });
        }
    }
}