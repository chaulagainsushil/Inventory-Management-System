using IMS.APPLICATION.Apllication.Repository;
using IMS.APPLICATION.Apllication.Services;
using IMS.APPLICATION.Application.Repository;
using IMS.APPLICATION.Application.Services;
using IMS.APPLICATION.Interface.Repository;
using IMS.APPLICATION.Interface.Services;
using Microsoft.Extensions.DependencyInjection;

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

            // 🔹 Register Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISupplierInformationService, SupplierInformationService>();
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}
