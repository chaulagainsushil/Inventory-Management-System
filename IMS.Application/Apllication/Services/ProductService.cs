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
                QuantityPerUnit =dto.QuantityPerUnit,
                Sku=dto.Sku,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(product);
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
    }
}
