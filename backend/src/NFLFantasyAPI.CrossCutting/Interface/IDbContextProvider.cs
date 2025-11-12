using Microsoft.Extensions.DependencyInjection;

namespace NFLFantasyAPI.CrossCutting.Interface
{
    public interface IDbContextProvider
    {
        void ConfigureDatabase(IServiceCollection services, string connectionString);
        public void registerRepositories(IServiceCollection services);
    }

}


