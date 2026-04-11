using IMS.COMMON.Dtos;
using IMS.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Interface.Repository
{
    public interface ISupplierRepository
    {
        Task<List<SuppliersInfromation>> GetAllAsync();
        Task<SuppliersInfromation?> GetByIdAsync(int id);
        Task AddAsync(SuppliersInfromation supplier);
        Task UpdateAsync(SuppliersInfromation supplier);
        Task DeleteAsync(int id);
        Task<int?> GetSupplierIdByNameAsync(string supplierName);
        Task<int> GetSupplierCountAsync();
        Task<IEnumerable<SupplierDropdownDto>> GetSupplierDropdownAsync();
    }
}
