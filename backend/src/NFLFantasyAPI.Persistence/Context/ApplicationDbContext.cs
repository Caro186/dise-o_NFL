using Microsoft.EntityFrameworkCore;
using NFLFantasyAPI.Persistence.Models;

namespace NFLFantasyAPI.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<EquipoNFL> EquiposNFL { get; set; }
        public DbSet<EquipoFantasy> EquiposFantasy { get; set; }
        public DbSet<Liga> Ligas { get; set; }
        public DbSet<Temporada> Temporadas { get; set; }
        public DbSet<Semana> Semanas { get; set; }
        public DbSet<Jugador> Jugadores { get; set; }  // NUEVO

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
                entity.Property(u => u.Rol).IsRequired().HasMaxLength(20).HasDefaultValue("Usuario");
            });

            // Configurar tabla de equipos NFL
            modelBuilder.Entity<EquipoNFL>(entity =>
            {
                entity.ToTable("equipos_nfl");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ciudad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImagenUrl).HasMaxLength(500);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");
                entity.Property(e => e.FechaCreacion).IsRequired();
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // Configurar tabla de equipos Fantasy
            modelBuilder.Entity<EquipoFantasy>(entity =>
            {
                entity.ToTable("equipos_fantasy");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UsuarioId).IsRequired();
                entity.Property(e => e.ImagenUrl).HasMaxLength(500);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");
                entity.Property(e => e.FechaCreacion).IsRequired();

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Liga)
                    .WithMany()
                    .HasForeignKey(e => e.LigaId)
                    .OnDelete(DeleteBehavior.SetNull);

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
                entity.Property(l => l.ComisionadoId).IsRequired();
                entity.Property(l => l.FormatoPosiciones).IsRequired();
                entity.Property(l => l.EsquemaPuntos).IsRequired();
                entity.Property(l => l.ConfigPlayoffs).IsRequired();
                entity.Property(l => l.PermitirDecimales).HasDefaultValue(true);

                entity.HasOne(l => l.Comisionado)
                    .WithMany()
                    .HasForeignKey(l => l.ComisionadoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Temporada)
                    .WithMany()
                    .HasForeignKey(l => l.IdTemporada)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(l => l.NombreLiga);
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

                entity.HasOne(s => s.Temporada)
                    .WithMany(t => t.Semanas)
                    .HasForeignKey(s => s.TemporadaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.TemporadaId);
            });

            // NUEVA CONFIGURACIÓN: Tabla de jugadores
            modelBuilder.Entity<Jugador>(entity =>
            {
                entity.ToTable("jugadores");
                entity.HasKey(j => j.Id);
                entity.Property(j => j.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(j => j.Posicion).IsRequired().HasMaxLength(50);
                entity.Property(j => j.EquipoNFLId).IsRequired();
                entity.Property(j => j.ImagenUrl).HasMaxLength(500);
                entity.Property(j => j.ThumbnailUrl).HasMaxLength(500);
                entity.Property(j => j.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");
                entity.Property(j => j.FechaCreacion).IsRequired();
                entity.Property(j => j.FechaActualizacion).IsRequired(false);

                // Relación con EquipoNFL
                entity.HasOne(j => j.EquipoNFL)
                    .WithMany()
                    .HasForeignKey(j => j.EquipoNFLId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices
                entity.HasIndex(j => j.EquipoNFLId);
                entity.HasIndex(j => new { j.Nombre, j.EquipoNFLId }).IsUnique();
                entity.HasIndex(j => j.Posicion);
                entity.HasIndex(j => j.Estado);
            });
        }
    }
}
