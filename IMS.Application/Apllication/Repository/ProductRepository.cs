using IMS.APPLICATION.Interface.Repository;
using IMS.Data;
using IMS.Models.Models;
using Microsoft.EntityFrameworkCore; // <-- Add this using directive
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Apllication.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Product
                .Where(p => p.IsActive)
                .Include(p => p.Category) // optional, if you want category details
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Product.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Product.FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                product.IsActive = false;
                _context.Product.Update(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            return await _context.Product.CountAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryNameAsync(string categoryName)
        {
            return await _context.Product
                .Include(p => p.Category)
                .Where(p => p.Category.Name.ToLower() == categoryName.ToLower()
                            && p.Category.IsActive)
                .ToListAsync();
        }
    }
}
