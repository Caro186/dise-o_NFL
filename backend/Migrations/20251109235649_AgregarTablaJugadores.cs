using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NFLFantasyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaJugadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jugadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Posicion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EquipoNFLId = table.Column<int>(type: "integer", nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jugadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_jugadores_equipos_nfl_EquipoNFLId",
                        column: x => x.EquipoNFLId,
                        principalTable: "equipos_nfl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_jugadores_EquipoNFLId",
                table: "jugadores",
                column: "EquipoNFLId");

            migrationBuilder.CreateIndex(
                name: "IX_jugadores_Estado",
                table: "jugadores",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_jugadores_Nombre_EquipoNFLId",
                table: "jugadores",
                columns: new[] { "Nombre", "EquipoNFLId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_jugadores_Posicion",
                table: "jugadores",
                column: "Posicion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jugadores");
        }
    }
}
