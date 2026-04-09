using IMS.APPLICATION.Apllication.Repository;
using IMS.APPLICATION.Apllication.Services;
using IMS.APPLICATION.Application.Repository;
using IMS.APPLICATION.Application.Services;
using IMS.APPLICATION.Interface.Repository;
using IMS.APPLICATION.Interface.Services;
using IMS.APPLICATION.Application.Services; // or the correct namespace where EmailService is defined

namespace IMS.COMMON.DependencyHandler
{
    public static class DependencyHandler
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 🔹 Register Repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISupplierRepository, SupplierInformationRepository>();
            services.AddScoped<IProductRepository , ProductRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IReportRepository , ReportRepository>();

            // 🔹 Register Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISupplierInformationService, SupplierInformationService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IEmailService, EmailService >();
            services.AddScoped<GmailTokenService>();
            services.AddScoped<IReportService,ReportService>();

            return services;
        }
    }
}
