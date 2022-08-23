using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Interface;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensios
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddAplicationService(this IServiceCollection services, IConfiguration configuration)
        {
             services.AddScoped<ITokenService,TokenService>();
            services.AddDbContext<DataContext>(options=>{
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            });
            return services;
        }
        
    }
}