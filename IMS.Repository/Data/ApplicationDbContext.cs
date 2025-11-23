using IMS.Models.Models;
using IMS.Models.Models.Identity;
using Microsoft.EntityFrameworkCore;

// Add-Migration Init -Project ims.Repository

namespace IMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<SuppliersInfromation> SuppliersInfromation { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<SignUpUser> SignUpUser { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }


    }
}