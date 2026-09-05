using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medialuncita.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarHistorialPrecios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistorialPreciosIngredientes_UnidadesMedida_UnidadId",
                table: "HistorialPreciosIngredientes");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialPreciosMateriales_UnidadesMedida_UnidadId",
                table: "HistorialPreciosMateriales");

            migrationBuilder.DropIndex(
                name: "IX_HistorialPreciosMateriales_UnidadId",
                table: "HistorialPreciosMateriales");

            migrationBuilder.DropIndex(
                name: "IX_HistorialPreciosIngredientes_UnidadId",
                table: "HistorialPreciosIngredientes");

            migrationBuilder.DropColumn(
                name: "Fuente",
                table: "HistorialPreciosMateriales");

            migrationBuilder.DropColumn(
                name: "UnidadId",
                table: "HistorialPreciosMateriales");

            migrationBuilder.DropColumn(
                name: "Fuente",
                table: "HistorialPreciosIngredientes");

            migrationBuilder.DropColumn(
                name: "UnidadId",
                table: "HistorialPreciosIngredientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fuente",
                table: "HistorialPreciosMateriales",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UnidadId",
                table: "HistorialPreciosMateriales",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Fuente",
                table: "HistorialPreciosIngredientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UnidadId",
                table: "HistorialPreciosIngredientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPreciosMateriales_UnidadId",
                table: "HistorialPreciosMateriales",
                column: "UnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPreciosIngredientes_UnidadId",
                table: "HistorialPreciosIngredientes",
                column: "UnidadId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialPreciosIngredientes_UnidadesMedida_UnidadId",
                table: "HistorialPreciosIngredientes",
                column: "UnidadId",
                principalTable: "UnidadesMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialPreciosMateriales_UnidadesMedida_UnidadId",
                table: "HistorialPreciosMateriales",
                column: "UnidadId",
                principalTable: "UnidadesMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
