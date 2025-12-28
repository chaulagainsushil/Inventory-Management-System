using IMS.APPLICATION.Interface.Repository;
using IMS.COMMON.Dtos;
using IMS.Data;
using IMS.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<List<CategoryDropdownDto>> GetDropdownAsync()
        {
            return await _context.Category
                .Where(x => x.IsActive)
                .Select(x => new CategoryDropdownDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        public async Task<int?> GetCategoryIdByNameAsync(string categoryName)
        {
            return await _context.Category
                .Where(x => x.Name == categoryName && x.IsActive)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCategoryCountAsync()
        {
            return await _context.Category.CountAsync(c => c.IsActive);
        }
    }
}
