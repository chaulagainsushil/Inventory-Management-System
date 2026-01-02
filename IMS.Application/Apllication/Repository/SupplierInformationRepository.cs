using IMS.APPLICATION.Interface.Repository;
using IMS.COMMON.Dtos;
using IMS.Data;
using IMS.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.APPLICATION.Apllication.Repository
{
    public class SupplierInformationRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierInformationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SuppliersInfromation>> GetAllAsync()
        {
            return await _context.SuppliersInfromation.ToListAsync();
        }

        public async Task<SuppliersInfromation?> GetByIdAsync(int id)
        {
            return await _context.SuppliersInfromation
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        }

        public async Task AddAsync(SuppliersInfromation supplier)
        {
            await _context.SuppliersInfromation.AddAsync(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SuppliersInfromation supplier)
        {
            _context.SuppliersInfromation.Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _context.SuppliersInfromation.FindAsync(id);
            if (supplier != null)
            {
                supplier.IsActive = false;
                _context.SuppliersInfromation.Update(supplier);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<SupplierDropdownDto>> GetDropdownAsync()
        {
            return await _context.SuppliersInfromation
                .Where(x => x.IsActive)
                .Select(x => new SupplierDropdownDto
                {
                    Id = x.Id,
                    SupplierName = x.Name
                })
                .ToListAsync();
        }

        public async Task<int?> GetSupplierIdByNameAsync(string Name)
        {
            return await _context.SuppliersInfromation
                .Where(x => x.Name == Name && x.IsActive)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetSupplierCountAsync()
        {
            return await _context.SuppliersInfromation
                .CountAsync(x => x.IsActive);
        }
    }
}
