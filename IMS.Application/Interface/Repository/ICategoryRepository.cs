using IMS.COMMON.Dtos;
using IMS.Models.Models;

namespace IMS.APPLICATION.Interface.Repository
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<int> GetCategoryCountAsync();
        Task<List<CategoryDropdownDto>> GetDropdownAsync();
        Task<int?> GetCategoryIdByNameAsync(string categoryName);
    }
}
