using IMS.APPLICATION.Interface.Repository;
using IMS.APPLICATION.Interface.Services;
using IMS.COMMON.Dtos;
using IMS.COMMON.Dtos.Identity;
using IMS.Models.Models;

namespace IMS.APPLICATION.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Category> CreateAsync(CategoryCreateDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(category);
            return category;
        }

        public async Task<Category?> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return null;

            category.Name = dto.Name;
            category.Description = dto.Description;
            await _repository.UpdateAsync(category);
            return category;
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