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
        public DbSet<Usuario> Usuarios { get; set; }

        /// <summary>
        /// Colección de equipos (VIEJO - mantener por compatibilidad)
        /// </summary>
        public DbSet<Equipo> Equipos { get; set; }

        /// <summary>
        /// Colección de equipos NFL (nuevos - equipos reales)
        /// </summary>
        public DbSet<EquipoNFL> EquiposNFL { get; set; }

        /// <summary>
        /// Colección de equipos Fantasy (nuevos - equipos de usuarios)
        /// </summary>
        public DbSet<EquipoFantasy> EquiposFantasy { get; set; }

        /// <summary>
        /// Colección de ligas en la base de datos
        /// </summary>
        public DbSet<Liga> Ligas { get; set; }

        /// <summary>
        /// Colección de temporadas en la base de datos
        /// </summary>
        public DbSet<Temporada> Temporadas { get; set; }

        /// <summary>
        /// Colección de semanas en la base de datos
        /// </summary>
        public DbSet<Semana> Semanas { get; set; }

        /// <summary>
        /// Colección de relaciones equipo-liga en la base de datos
        /// </summary>
        public DbSet<EquipoLiga> EquiposLigas { get; set; }

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
                entity.Property(u => u.IntentosFailidos).HasDefaultValue(0);
                entity.Property(u => u.FechaUltimoIntentoFallido).IsRequired(false);
                entity.Property(u => u.EstadoCuenta).IsRequired().HasMaxLength(20).HasDefaultValue("Activa");
                entity.Property(u => u.FechaBloqueo).IsRequired(false);
                entity.Property(u => u.UltimaActividad).IsRequired(false);
            });

            // Configurar tabla de equipos (VIEJO - mantener por compatibilidad)
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

            // 👇 NUEVA: Configurar tabla de equipos NFL (reales)
            modelBuilder.Entity<EquipoNFL>(entity =>
            {
                entity.ToTable("equipos_nfl");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ciudad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImagenUrl).HasMaxLength(500);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");
                entity.Property(e => e.FechaCreacion).IsRequired();
                
                // Índice único para búsquedas por nombre
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // 👇 NUEVA: Configurar tabla de equipos Fantasy (de usuarios)
            modelBuilder.Entity<EquipoFantasy>(entity =>
            {
                entity.ToTable("equipos_fantasy");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UsuarioId).IsRequired();
                entity.Property(e => e.ImagenUrl).HasMaxLength(500);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");
                entity.Property(e => e.FechaCreacion).IsRequired();
                
                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con Liga (opcional)
                entity.HasOne(e => e.Liga)
                    .WithMany()
                    .HasForeignKey(e => e.LigaId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Índice para búsquedas por usuario
                entity.HasIndex(e => e.UsuarioId);
            });

            // Configurar tabla de ligas
            modelBuilder.Entity<Liga>(entity =>
            {
                entity.ToTable("ligas");
                entity.HasKey(l => l.IdLiga);
                entity.Property(l => l.NombreLiga).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Descripcion).IsRequired(false);
                entity.Property(l => l.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(l => l.IdTemporada).IsRequired();
                entity.Property(l => l.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Pre-Draft");
                entity.Property(l => l.CuposTotales).IsRequired();
                entity.Property(l => l.CuposOcupados).HasDefaultValue(1);
                entity.Property(l => l.FechaCreacion).IsRequired();
                entity.Property(l => l.IdComisionado).IsRequired();
                entity.Property(l => l.FormatoPosiciones).IsRequired();
                entity.Property(l => l.EsquemaPuntos).IsRequired();
                entity.Property(l => l.ConfigPlayoffs).IsRequired();
                entity.Property(l => l.PermitirDecimales).HasDefaultValue(true);

                // Relación con Usuario (comisionado)
                entity.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(l => l.IdComisionado)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Temporada
                entity.HasOne<Temporada>()
                    .WithMany()
                    .HasForeignKey(l => l.IdTemporada)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice para búsquedas por nombre
                entity.HasIndex(l => l.NombreLiga);
            });

            // Configurar tabla de equipo-liga
            modelBuilder.Entity<EquipoLiga>(entity =>
            {
                entity.ToTable("equipos_ligas");
                entity.HasKey(el => el.Id);
                entity.Property(el => el.IdEquipo).IsRequired();
                entity.Property(el => el.IdLiga).IsRequired();
                entity.Property(el => el.Alias).IsRequired().HasMaxLength(50);
                entity.Property(el => el.FechaUnion).IsRequired();
                entity.Property(el => el.EsComisionado).IsRequired().HasDefaultValue(false);

                // Relaciones
                entity.HasOne(el => el.Equipo)
                    .WithMany()
                    .HasForeignKey(el => el.IdEquipo)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(el => el.Liga)
                    .WithMany()
                    .HasForeignKey(el => el.IdLiga)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice único: un equipo no puede estar más de una vez en la misma liga
                entity.HasIndex(el => new { el.IdEquipo, el.IdLiga }).IsUnique();
            });

            // Configurar tabla de temporadas
            modelBuilder.Entity<Temporada>(entity =>
            {
                entity.ToTable("temporadas");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Nombre).IsRequired();
                entity.Property(t => t.FechaInicio).IsRequired();
                entity.Property(t => t.FechaCierre).IsRequired();
                entity.Property(t => t.FechaCreacion).IsRequired();
                entity.Property(t => t.Actual).HasDefaultValue(false);
            });

            // Configurar tabla de semanas
            modelBuilder.Entity<Semana>(entity =>
            {
                entity.ToTable("semanas");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.TemporadaId).IsRequired();
                entity.Property(s => s.FechaInicio).IsRequired();
                entity.Property(s => s.FechaFin).IsRequired();

                // Relación con Temporada
                entity.HasOne<Temporada>()
                    .WithMany()
                    .HasForeignKey(s => s.TemporadaId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice para búsquedas por temporada
                entity.HasIndex(s => s.TemporadaId);
            });
        }
    }
}