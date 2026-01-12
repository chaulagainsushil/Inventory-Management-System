using IMS.APPLICATION.Apllication.Services;
using IMS.APPLICATION.Interface.Services;
using IMS.COMMON.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.GetAllAsync();
            return Ok(products);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null)
                return NotFound("Product not found");
            return Ok(product);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            var product = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound("Product not found");
            return Ok(updated);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound("Product not found");
            return Ok("Product deleted successfully");
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("Productcount")]
        public async Task<IActionResult> GetTotalProductCount()
        {
            var result = await _service.GetProductCountAsync();
            return Ok(result);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("reorder-alerts")]
        public async Task<IActionResult> GetReorderAlerts()
        {
            var products = await _service.GetAllAsync();

            var alerts = products
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    productId = p.Id,
                    productName = p.ProductName,
                    currentStock = p.StockQuantity,
                    reorderPoint = (int)(p.AverageDailySales * p.LeadTimeDays) + p.SafetyStock,
                    needsReorder = p.StockQuantity <= ((p.AverageDailySales * p.LeadTimeDays) + p.SafetyStock)
                })
                .Where(x => x.needsReorder)
                .ToList();

            return Ok(alerts);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategoryName([FromQuery] string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return BadRequest("Category name is required");

            var result = await _service.GetProductsByCategoryNameAsync(categoryName);

            if (!result.Any())
                return NotFound("No products found for this category");

            return Ok(result);
        }


        [HttpGet("products-by-category")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<ActionResult<ResponseDto>> GetProductsByCategory()
        {
            try
            {
                var result = await _service.GetProductsByCategoryAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Error retrieving products by category",
                    Error = ex.Message
                });
            }




        }
        [HttpGet("Productdropdown")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetProductDropdown()
        {
            var products = await _service.GetProductDropdownAsync();
            return Ok(products);
        }
    }
}


