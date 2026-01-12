using IMS.APPLICATION.Apllication.Repository;
using IMS.APPLICATION.Interface.Repository;
using IMS.APPLICATION.Interface.Services;
using IMS.COMMON.Dtos;
using IMS.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Apllication.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Product> CreateAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                ProductName = dto.ProductName,
                Description = dto.Description,
                PricePerUnit = dto.PricePerUnit,
                SuppliersInfromationId = dto.SupplierId,
                PricePerUnitPurchased = dto.PricePerUnitPurchased,

                Sku = dto.Sku,
                CategoryId = dto.CategoryId,
                StockQuantity = dto.StockQuantity, // add this to DTO first
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                // ✅ ROP Fields
                SafetyStock = dto.SafetyStock,
                LeadTimeDays = dto.LeadTimeDays,
                AverageDailySales = 0  // Initial value
            };

            await _repository.AddAsync(product); // ensure AddAsync actually saves  
            return product;
        }

        public async Task<Product?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;

            product.ProductName = dto.ProductName;
            product.Description = dto.Description;
            product.PricePerUnit = dto.PricePerUnit;
            product.CategoryId = dto.CategoryId;

            await _repository.UpdateAsync(product);
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<ProductCountDto> GetProductCountAsync()
        {
            int count = await _repository.GetTotalProductCountAsync();

            return new ProductCountDto
            {
                TotalProductCount = count
            };
        }
        public async Task<List<ProductDto>> GetProductsByCategoryNameAsync(string categoryName)
        {
            var products = await _repository.GetProductsByCategoryNameAsync(categoryName);

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.ProductName,
                Price = p.PricePerUnit,
                CategoryName = p.Category.Name
            }).ToList();
        }

        public async Task<ResponseDto> GetProductsByCategoryAsync()
        {
            var response = new ResponseDto();

            try
            {
                // Get data from repository
                var productsByCategory = await _repository.GetProductsByCategoryAsync();

                if (productsByCategory == null || !productsByCategory.Any())
                {
                    response.IsSuccess = true;
                    response.Message = "No products found";
                    response.Data = new List<CategoryProductDto>();
                    return response;
                }

                // Calculate total products
                int totalProducts = productsByCategory.Sum(x => x.ProductCount);

                // Calculate percentage for each category
                var result = productsByCategory.Select(c => new CategoryProductDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = c.ProductCount,
                    PercentageOfTotal = totalProducts > 0
                        ? Math.Round((c.ProductCount * 100m) / totalProducts, 2)
                        : 0
                }).OrderByDescending(x => x.ProductCount).ToList();

                response.IsSuccess = true;
                response.Message = "Products by category retrieved successfully";
                response.Data = new
                {
                    TotalProducts = totalProducts,
                    Categories = result
                };
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "An error occurred while retrieving products by category";
                response.Error = ex.Message;
            }

            return response;
        }
        public async Task<List<ProductDropdownDto>> GetProductDropdownAsync()
        {
            return await _repository.GetProductDropdownAsync();
        }
    }
}

