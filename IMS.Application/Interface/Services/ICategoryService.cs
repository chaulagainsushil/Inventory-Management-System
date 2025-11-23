using IMS.COMMON.Dtos;
using IMS.COMMON.Dtos.Identity;
using IMS.Models.Models;

namespace IMS.APPLICATION.Interface.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(CategoryCreateDto dto);
        Task<Category?> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}