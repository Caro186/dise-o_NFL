using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.CrossCutting.Interface; 
using NFLFantasyAPI.Persistence.Context; 

namespace NFLFantasyAPI.Logic.DbContextProvider 
{
    public class DbContextProvider : IDbContextProvider 
    { 
        public void ConfigureDatabase(IServiceCollection services, string connectionString) 
        { 
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString)); 
        } 
    } 
}
