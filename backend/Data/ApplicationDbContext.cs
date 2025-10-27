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
        public DbSet<Liga> Ligas { get; set; }
        public DbSet<Temporada> Temporadas { get; set; }
        public DbSet<Semana> Semanas { get; set; }

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

            // Configuración de Temporada
            modelBuilder.Entity<Temporada>(entity =>
            {
                entity.ToTable("temporadas");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Nombre).IsRequired();
                entity.Property(t => t.FechaInicio).IsRequired();
                entity.Property(t => t.FechaCierre).IsRequired();
                entity.Property(t => t.FechaCreacion).IsRequired();
                entity.Property(t => t.Actual).HasDefaultValue(false);

                // Relación uno a muchos con Semana
                entity.HasMany(t => t.Semanas)
                      .WithOne(s => s.Temporada)
                      .HasForeignKey(s => s.TemporadaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Semana
            modelBuilder.Entity<Semana>(entity =>
            {
                entity.ToTable("semanas");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.FechaInicio).IsRequired();
                entity.Property(s => s.FechaFin).IsRequired();
                entity.Property(s => s.TemporadaId).IsRequired();
            });

        }
    }
}
