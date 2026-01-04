using IMS.Models.Models;
using IMS.Models.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IMS.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<SuppliersInfromation> SuppliersInfromation { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Sale> Sale { get; set; }
        public DbSet<SaleItem> SaleItem { get; set; }

        public DbSet<InventoryBatch> InventoryBatch { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer - Unique phone number
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber)
                .IsUnique();

            // Product - Unique SKU
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            // Sale - Customer relationship (optional)
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // SaleItem relationships
            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
