using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Models;

namespace NFLFantasyAPI.Data
{
    /// <summary>
    /// Contexto de base de datos para la aplicación Fantasy NFL
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor que recibe las opciones de configuración del contexto
        /// </summary>
        /// <param name="options">Opciones de configuración del DbContext</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Colección de usuarios en la base de datos
        /// </summary>
        public DbSet<Usuario> Usuarios { get; set; } //esto crea una tabla en la db

        /// <summary>
        /// Colección de equipos en la base de datos
        /// </summary>
        public DbSet<Equipo> Equipos { get; set; }

        /// <summary>
        /// Configuración adicional del modelo de datos
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar tabla de usuarios
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(255);
                entity.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(50);
                entity.Property(u => u.FechaRegistro).IsRequired();
            });

            // Configurar tabla de equipos
            modelBuilder.Entity<Equipo>(entity =>
            {
                entity.ToTable("equipos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImagenUrl).HasMaxLength(500);
                entity.Property(e => e.FechaCreacion).IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");
                entity.Property(e => e.Liga).HasMaxLength(50);

                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice para mejorar búsquedas por usuario
                entity.HasIndex(e => e.UsuarioId);
            });
        }
    }
}