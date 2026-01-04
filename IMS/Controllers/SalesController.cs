using IMS.APPLICATION.Apllication.Services;
using IMS.COMMON.Dtos;
using IMS.Data;
using IMS.Models.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Check if customer exists by phone number
        /// </summary>

        [HttpGet("check-customer/{phoneNumber}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CheckCustomer(string phoneNumber)
        {
            var customer = await _context.Customer
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (customer != null)
            {
                return Ok(new
                {
                    exists = true,
                    customer = new
                    {
                        customerId = customer.Id,
                        customerName = customer.CustomerName,
                        phoneNumber = customer.PhoneNumber,
                        email = customer.Email,
                        address = customer.Address
                    }
                });
            }

            return Ok(new
            {
                exists = false,
                message = "Customer not found. Please provide details to create new customer."
            });
        }

        /// <summary>
        /// Create a new sale with automatic customer handling
        /// </summary>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateSale(SaleCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int? customerId = null;

                // 🔍 Customer Handling Logic
                if (!string.IsNullOrEmpty(dto.CustomerPhoneNumber))
                {
                    var existingCustomer = await _context.Customer
                        .FirstOrDefaultAsync(c => c.PhoneNumber == dto.CustomerPhoneNumber);

                    if (existingCustomer != null)
                    {
                        // Customer exists
                        customerId = existingCustomer.Id;
                    }
                    else
                    {
                        // Customer doesn't exist - create new
                        if (dto.NewCustomer == null)
                        {
                            return BadRequest(new
                            {
                                error = "Customer not found",
                                phoneNumber = dto.CustomerPhoneNumber,
                                message = "Please provide customer details to create new customer",
                                requiresCustomerInfo = true
                            });
                        }

                        if (string.IsNullOrEmpty(dto.NewCustomer.CustomerName))
                            return BadRequest("Customer name is required");

                        var newCustomer = new Customer
                        {
                            CustomerName = dto.NewCustomer.CustomerName,
                            PhoneNumber = dto.CustomerPhoneNumber,
                            Email = dto.NewCustomer.Email,
                            Address = dto.NewCustomer.Address,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Customer.Add(newCustomer);
                        await _context.SaveChangesAsync();

                        customerId = newCustomer.Id;
                    }
                }

                // 📦 Process Sale Items
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
                        return BadRequest($"Insufficient stock for {product.ProductName}. Available: {product.StockQuantity}");

                    // Update stock
                    product.StockQuantity -= item.Quantity;

                    // Update average daily sales
                    product.AverageDailySales = await CalculateAverageDailySales(product.Id);

                    // ✅ DYNAMIC ROP CALCULATION
                    int calculatedROP = (int)(product.AverageDailySales * product.LeadTimeDays)
                                        + product.SafetyStock;

                    // Check if reorder needed
                    if (product.StockQuantity <= calculatedROP)
                    {
                        int suggestedQty = Math.Max(
                            calculatedROP + product.SafetyStock - product.StockQuantity,
                            (int)(product.AverageDailySales * 7)
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

                // 💰 Create Sale
                var sale = new Sale
                {
                    InvoiceNo = $"INV-{DateTime.Now.Ticks}",
                    SaleDate = DateTime.UtcNow,
                    CustomerId = customerId,
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
                    saleId = sale.SaleId,
                    total = sale.TotalAmount,
                    customerId = customerId,
                    customerCreated = dto.NewCustomer != null,
                    reorderAlerts = lowStockProducts,
                    alertCount = lowStockProducts.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
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
                return "CRITICAL";
            else if (currentStock <= rop * 0.5)
                return "HIGH";
            else if (currentStock <= rop * 0.75)
                return "MEDIUM";
            else
                return "LOW";
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
                alerts = alerts.OrderBy(a =>
                    ((dynamic)a).urgencyLevel == "CRITICAL" ? 0 :
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

            return Ok(new
            {
                totalSales = totalSale,
                month = now.ToString("MMMM yyyy")
            });
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

        /// <summary>
        /// Get sales history with customer info
        /// </summary>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetSales([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var sales = await _context.Sale
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    saleId = s.SaleId,
                    invoiceNo = s.InvoiceNo,
                    saleDate = s.SaleDate,
                    customerName = s.Customer != null ? s.Customer.CustomerName : "Walk-in",
                    customerPhone = s.Customer != null ? s.Customer.PhoneNumber : null,
                    totalAmount = s.TotalAmount,
                    paymentMethod = s.PaymentMethod,
                    status = s.Status,
                    itemCount = s.SaleItems.Count
                })
                .ToListAsync();

            var totalCount = await _context.Sale.CountAsync();

            return Ok(new
            {
                sales,
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }
        /// <summary>
        /// Get current month revenue
        /// </summary>
        [HttpGet("monthly-revenue")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCurrentMonthRevenue()
        {
            var now = DateTime.Now;

            var revenue = await _context.Sale
                .Where(s => s.SaleDate.Year == now.Year &&
                            s.SaleDate.Month == now.Month)
                .SumAsync(s => s.SubTotal);

            return Ok(new
            {
                Month = now.ToString("MMMM"),
                Year = now.Year,
                Revenue = revenue
            });
        }

        [HttpGet("top-selling-products")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<ActionResult<IEnumerable<ProductSalesDto>>> GetTopSellingProducts(int topCount = 10)
        {
            try
            {
                // Get total quantity sold
                var totalQuantitySold = await _context.SaleItem.SumAsync(si => si.Quantity);

                if (totalQuantitySold == 0)
                {
                    return Ok(new List<ProductSalesDto>());
                }

                // Get top selling products
                var topProducts = await _context.SaleItem
                    .Include(si => si.Product)
                    .GroupBy(si => new { si.ProductId, si.Product.ProductName })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        TotalQuantity = g.Sum(si => si.Quantity),
                        TotalSalesAmount = g.Sum(si => si.Quantity * si.UnitPrice)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(topCount)
                    .ToListAsync();

                // Map to DTO with percentage calculation
                var result = topProducts.Select(p => new ProductSalesDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    TotalQuantitySold = p.TotalQuantity,
                    SalesPercentage = Math.Round((p.TotalQuantity * 100m) / totalQuantitySold, 2),
                    TotalSalesAmount = p.TotalSalesAmount
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving top selling products", error = ex.Message });
            }
        }

        /// <summary>
        /// Get products sales with date range filter
        /// </summary>
        [HttpGet("top-selling-products-by-date")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<ActionResult<IEnumerable<ProductSalesDto>>> GetTopSellingProductsByDate(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int topCount = 10)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(new { message = "Start date must be before end date" });
                }

                // Get total quantity sold in date range
                var totalQuantitySold = await _context.SaleItem
                    .Where(si => si.Sale.SaleDate >= startDate && si.Sale.SaleDate <= endDate)
                    .SumAsync(si => si.Quantity);

                if (totalQuantitySold == 0)
                {
                    return Ok(new List<ProductSalesDto>());
                }

                // Get top selling products by date range
                var topProducts = await _context.SaleItem
                    .Where(si => si.Sale.SaleDate >= startDate && si.Sale.SaleDate <= endDate)
                    .Include(si => si.Product)
                    .Include(si => si.Sale)
                    .GroupBy(si => new { si.ProductId, si.Product.ProductName })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        TotalQuantity = g.Sum(si => si.Quantity),
                        TotalSalesAmount = g.Sum(si => si.Quantity * si.UnitPrice)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(topCount)
                    .ToListAsync();

                var result = topProducts.Select(p => new ProductSalesDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    TotalQuantitySold = p.TotalQuantity,
                    SalesPercentage = Math.Round((p.TotalQuantity * 100m) / totalQuantitySold, 2),
                    TotalSalesAmount = p.TotalSalesAmount
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving top selling products", error = ex.Message });
            }
        }
    }
}