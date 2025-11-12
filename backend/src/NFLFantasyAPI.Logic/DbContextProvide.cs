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
            // Repositorios
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEquipoFantasyRepository, EquipoFantasyRepository>();
            services.AddScoped<IEquipoNFLRepository, EquipoNFLRepository>();
            services.AddScoped<IJugadorRepository, JugadorRepository>();
            services.AddScoped<ILigaRepository, LigaRepository>();
            services.AddScoped<ITemporadaRepository, TemporadaRepository>();
        }
    }
}
