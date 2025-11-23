using IMS.COMMON.Dtos;
using IMS.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Interface.Services
{
    public interface ISupplierInformationService
    {
        Task<IEnumerable<SuppliersInfromation>> GetAllAsync();
        Task<SuppliersInfromation?> GetByIdAsync(int id);
        Task<SuppliersInfromation> CreateAsync(SupplierInformationDto dto);
        Task<SuppliersInfromation?> UpdateAsync(int id, SupplierInformationDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
