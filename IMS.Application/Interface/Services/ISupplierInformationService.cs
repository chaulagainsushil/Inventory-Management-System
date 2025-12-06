using IMS.COMMON.Dtos;
using IMS.COMMON.Dtos.Identity;
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
        Task<List<SupplireInfromationDisplayDto>> GetAllAsync();
        Task<SuppliersInfromation?> GetByIdAsync(int id);
        Task<SuppliersInfromation> CreateAsync(SupplierInformationDto dto);
        Task<SuppliersInfromation?> UpdateAsync(int id, SupplireInfromationDisplayDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
