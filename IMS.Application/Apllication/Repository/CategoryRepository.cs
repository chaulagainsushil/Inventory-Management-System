using IMS.APPLICATION.Interface.Repository;
using IMS.Data;
using IMS.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IMS.APPLICATION.Application.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            // ✅ Using LINQ Lambda Expression
            return await _context.Category
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Category
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Category.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Category.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Category.FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                category.IsActive = false;
                _context.Category.Update(category);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> GetCategoryCountAsync()
        {
            return await _context.Category.CountAsync();
        }
    }
}
