using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Models;

namespace NFLFantasyAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor que recibe las opciones de configuración
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Cada DbSet representa una tabla
        public DbSet<Usuario> Usuarios { get; set; }
    }
}