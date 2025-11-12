using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NFLFantasyAPI.Migrations
{
    /// <inheritdoc />
    public partial class cambiarTemporada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipos_nfl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Activo")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipos_nfl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "temporadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Actual = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temporadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IntentosFailidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FechaUltimoIntentoFallido = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstadoCuenta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Activa"),
                    FechaBloqueo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaActividad = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Usuario")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "semanas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TemporadaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_semanas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_semanas_temporadas_TemporadaId",
                        column: x => x.TemporadaId,
                        principalTable: "temporadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ligas",
                columns: table => new
                {
                    IdLiga = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NombreLiga = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IdTemporada = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pre-Draft"),
                    CuposTotales = table.Column<int>(type: "integer", nullable: false),
                    CuposOcupados = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ComisionadoId = table.Column<int>(type: "integer", nullable: false),
                    FormatoPosiciones = table.Column<string>(type: "text", nullable: false),
                    EsquemaPuntos = table.Column<string>(type: "text", nullable: false),
                    ConfigPlayoffs = table.Column<string>(type: "text", nullable: false),
                    PermitirDecimales = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ligas", x => x.IdLiga);
                    table.ForeignKey(
                        name: "FK_ligas_temporadas_IdTemporada",
                        column: x => x.IdTemporada,
                        principalTable: "temporadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ligas_usuarios_ComisionadoId",
                        column: x => x.ComisionadoId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipos_fantasy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    LigaId = table.Column<int>(type: "integer", nullable: true),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Activo")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipos_fantasy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_equipos_fantasy_ligas_LigaId",
                        column: x => x.LigaId,
                        principalTable: "ligas",
                        principalColumn: "IdLiga",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_equipos_fantasy_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_equipos_fantasy_LigaId",
                table: "equipos_fantasy",
                column: "LigaId");

            migrationBuilder.CreateIndex(
                name: "IX_equipos_fantasy_UsuarioId",
                table: "equipos_fantasy",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_equipos_nfl_Nombre",
                table: "equipos_nfl",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ligas_ComisionadoId",
                table: "ligas",
                column: "ComisionadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ligas_IdTemporada",
                table: "ligas",
                column: "IdTemporada");

            migrationBuilder.CreateIndex(
                name: "IX_ligas_NombreLiga",
                table: "ligas",
                column: "NombreLiga");

            migrationBuilder.CreateIndex(
                name: "IX_semanas_TemporadaId",
                table: "semanas",
                column: "TemporadaId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipos_fantasy");

            migrationBuilder.DropTable(
                name: "equipos_nfl");

            migrationBuilder.DropTable(
                name: "semanas");

            migrationBuilder.DropTable(
                name: "ligas");

            migrationBuilder.DropTable(
                name: "temporadas");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
