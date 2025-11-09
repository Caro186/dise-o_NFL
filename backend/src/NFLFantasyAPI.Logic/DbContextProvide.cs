using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.CrossCutting.Interface;
using NFLFantasyAPI.Persistence.Context;
using NFLFantasyAPI.Persistence.Repositories;
using NFLFantasyAPI.Persistence.Interfaces;

namespace NFLFantasyAPI.Logic.DbContextProvider
{
    public class DbContextProvider : IDbContextProvider
    {
        public void ConfigureDatabase(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        }

        public void registerRepositories(IServiceCollection services)
        {
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEquipoRepository, EquipoRepository>();
            services.AddScoped<ILigaRepository, LigaRepository>();
            services.AddScoped<ITemporadaRepository, TemporadaRepository>();
            services.AddScoped<IEquipoLigaRepository, EquipoLigaRepository>();
        }
    }
}
