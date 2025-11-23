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
    public class SupplierInformationService : ISupplierInformationService
    {
        private readonly ISupplierRepository _repository;

        public SupplierInformationService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SuppliersInfromation>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SuppliersInfromation?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<SuppliersInfromation> CreateAsync(SupplierInformationDto dto)
        {
            var supplier = new SuppliersInfromation
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(supplier);
            return supplier;
        }

        public async Task<SuppliersInfromation?> UpdateAsync(int id, SupplierInformationDto dto)
        {
            var supplier = await _repository.GetByIdAsync(id);
            if (supplier == null) return null;

            supplier.Name = dto.Name;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.PhoneNumber = dto.PhoneNumber;
            supplier.Email = dto.Email;
            supplier.Address = dto.Address;

            await _repository.UpdateAsync(supplier);
            return supplier;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var SuppliersInfromation = await _repository.GetByIdAsync(id);
            if (SuppliersInfromation == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
