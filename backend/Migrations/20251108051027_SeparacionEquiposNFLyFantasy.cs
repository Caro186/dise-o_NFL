using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NFLFantasyAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeparacionEquiposNFLyFantasy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TemporadaId1",
                table: "semanas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_semanas_TemporadaId1",
                table: "semanas",
                column: "TemporadaId1");

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

            migrationBuilder.AddForeignKey(
                name: "FK_semanas_temporadas_TemporadaId1",
                table: "semanas",
                column: "TemporadaId1",
                principalTable: "temporadas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_semanas_temporadas_TemporadaId1",
                table: "semanas");

            migrationBuilder.DropTable(
                name: "equipos_fantasy");

            migrationBuilder.DropTable(
                name: "equipos_nfl");

            migrationBuilder.DropIndex(
                name: "IX_semanas_TemporadaId1",
                table: "semanas");

            migrationBuilder.DropColumn(
                name: "TemporadaId1",
                table: "semanas");
        }
    }
}
