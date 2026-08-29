using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medialuncita.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionGlobal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TarifaManoDeObraPorHora = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    EstrategiaPrecioDefault = table.Column<int>(type: "INTEGER", nullable: false),
                    MargenPorcentualDefault = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MultiplicadorDefault = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    EstrategiaRedondeoDefault = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionGlobal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClienteNombre = table.Column<string>(type: "TEXT", nullable: true),
                    Notas = table.Column<string>(type: "TEXT", nullable: true),
                    Total = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    CostoPorHora = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    CostoPorLote = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Abreviatura = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    FactorAUnidadBase = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PresupuestoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoVarianteId = table.Column<int>(type: "INTEGER", nullable: true),
                    NombreProductoSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    NombreVarianteSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CostoIngredientesSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CostoPackagingSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CostoManoDeObraSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CostoServiciosSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CostoTotalSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CostoUnitarioSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    TiempoTotalMinutosSnapshot = table.Column<int>(type: "INTEGER", nullable: false),
                    TarifaManoDeObraPorHoraSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    EstrategiaPrecioSnapshot = table.Column<int>(type: "INTEGER", nullable: false),
                    MargenPorcentualSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    MultiplicadorSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    EstrategiaRedondeoSnapshot = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitarioAlMomento = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Subtotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresupuestoItems_Presupuestos_PresupuestoId",
                        column: x => x.PresupuestoId,
                        principalTable: "Presupuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingredientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", nullable: true),
                    UnidadCompraId = table.Column<int>(type: "INTEGER", nullable: false),
                    MermaDefault = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    DensidadGramosPorMililitro = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredientes_UnidadesMedida_UnidadCompraId",
                        column: x => x.UnidadCompraId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materiales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", nullable: true),
                    UnidadCompraId = table.Column<int>(type: "INTEGER", nullable: false),
                    MermaDefault = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materiales_UnidadesMedida_UnidadCompraId",
                        column: x => x.UnidadCompraId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    RendimientoBaseCantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    RendimientoBaseUnidadId = table.Column<int>(type: "INTEGER", nullable: false),
                    TiempoPreparacionBaseMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recetas_UnidadesMedida_RendimientoBaseUnidadId",
                        column: x => x.RendimientoBaseUnidadId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestoItemIngredienteDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PresupuestoItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreIngredienteSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    CantidadRequeridaSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MermaAplicadaSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CantidadEfectivaSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    UnidadSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    PrecioUnitarioUsadoSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    SubtotalSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestoItemIngredienteDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresupuestoItemIngredienteDetalles_PresupuestoItems_PresupuestoItemId",
                        column: x => x.PresupuestoItemId,
                        principalTable: "PresupuestoItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestoItemMaterialDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PresupuestoItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreMaterialSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    CantidadRequeridaSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MermaAplicadaSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CantidadEfectivaSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    UnidadSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    PrecioUnitarioUsadoSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    SubtotalSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestoItemMaterialDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresupuestoItemMaterialDetalles_PresupuestoItems_PresupuestoItemId",
                        column: x => x.PresupuestoItemId,
                        principalTable: "PresupuestoItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestoItemServicioDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PresupuestoItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreServicioSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    ModoProrrateoSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    SubtotalSnapshot = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestoItemServicioDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresupuestoItemServicioDetalles_PresupuestoItems_PresupuestoItemId",
                        column: x => x.PresupuestoItemId,
                        principalTable: "PresupuestoItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialPreciosIngredientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IngredienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    UnidadId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fuente = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialPreciosIngredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialPreciosIngredientes_Ingredientes_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingredientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialPreciosIngredientes_UnidadesMedida_UnidadId",
                        column: x => x.UnidadId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialPreciosMateriales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    UnidadId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fuente = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialPreciosMateriales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialPreciosMateriales_Materiales_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialPreciosMateriales_UnidadesMedida_UnidadId",
                        column: x => x.UnidadId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    RecetaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "Recetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecetaIngredientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecetaId = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    UnidadId = table.Column<int>(type: "INTEGER", nullable: false),
                    MermaOverride = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetaIngredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecetaIngredientes_Ingredientes_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingredientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecetaIngredientes_Recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "Recetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecetaIngredientes_UnidadesMedida_UnidadId",
                        column: x => x.UnidadId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecetaServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecetaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModoProrrateo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetaServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecetaServicios_Recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "Recetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecetaServicios_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductoVariantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    RendimientoCantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    RendimientoUnidadId = table.Column<int>(type: "INTEGER", nullable: false),
                    TiempoAdicionalPorLoteMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    TiempoAdicionalPorUnidadMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    EstrategiaPrecioOverride = table.Column<int>(type: "INTEGER", nullable: true),
                    MargenPorcentualOverride = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    MultiplicadorOverride = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    PrecioManualOverride = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    EstrategiaRedondeoOverride = table.Column<int>(type: "INTEGER", nullable: true),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoVariantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoVariantes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoVariantes_UnidadesMedida_RendimientoUnidadId",
                        column: x => x.RendimientoUnidadId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VarianteIngredienteOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VarianteId = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    CantidadOverride = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    UnidadId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarianteIngredienteOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VarianteIngredienteOverrides_Ingredientes_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingredientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarianteIngredienteOverrides_ProductoVariantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "ProductoVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VarianteIngredienteOverrides_UnidadesMedida_UnidadId",
                        column: x => x.UnidadId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VarianteMateriales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VarianteId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarianteMateriales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VarianteMateriales_Materiales_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarianteMateriales_ProductoVariantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "ProductoVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VarianteServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VarianteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModoProrrateo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarianteServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VarianteServicios_ProductoVariantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "ProductoVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VarianteServicios_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ConfiguracionGlobal",
                columns: new[] { "Id", "EstrategiaPrecioDefault", "EstrategiaRedondeoDefault", "MargenPorcentualDefault", "MultiplicadorDefault", "TarifaManoDeObraPorHora" },
                values: new object[] { 1, 1, 0, 0m, 1m, 0m });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPreciosIngredientes_IngredienteId_Fecha",
                table: "HistorialPreciosIngredientes",
                columns: new[] { "IngredienteId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPreciosIngredientes_UnidadId",
                table: "HistorialPreciosIngredientes",
                column: "UnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPreciosMateriales_MaterialId_Fecha",
                table: "HistorialPreciosMateriales",
                columns: new[] { "MaterialId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPreciosMateriales_UnidadId",
                table: "HistorialPreciosMateriales",
                column: "UnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_Nombre",
                table: "Ingredientes",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_UnidadCompraId",
                table: "Ingredientes",
                column: "UnidadCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_Materiales_UnidadCompraId",
                table: "Materiales",
                column: "UnidadCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestoItemIngredienteDetalles_PresupuestoItemId",
                table: "PresupuestoItemIngredienteDetalles",
                column: "PresupuestoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestoItemMaterialDetalles_PresupuestoItemId",
                table: "PresupuestoItemMaterialDetalles",
                column: "PresupuestoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestoItems_PresupuestoId",
                table: "PresupuestoItems",
                column: "PresupuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestoItemServicioDetalles_PresupuestoItemId",
                table: "PresupuestoItemServicioDetalles",
                column: "PresupuestoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_RecetaId",
                table: "Productos",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoVariantes_ProductoId",
                table: "ProductoVariantes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoVariantes_RendimientoUnidadId",
                table: "ProductoVariantes",
                column: "RendimientoUnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaIngredientes_IngredienteId",
                table: "RecetaIngredientes",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaIngredientes_RecetaId",
                table: "RecetaIngredientes",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaIngredientes_UnidadId",
                table: "RecetaIngredientes",
                column: "UnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Recetas_RendimientoBaseUnidadId",
                table: "Recetas",
                column: "RendimientoBaseUnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaServicios_RecetaId",
                table: "RecetaServicios",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaServicios_ServicioId",
                table: "RecetaServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_Nombre",
                table: "UnidadesMedida",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VarianteIngredienteOverrides_IngredienteId",
                table: "VarianteIngredienteOverrides",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianteIngredienteOverrides_UnidadId",
                table: "VarianteIngredienteOverrides",
                column: "UnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianteIngredienteOverrides_VarianteId",
                table: "VarianteIngredienteOverrides",
                column: "VarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianteMateriales_MaterialId",
                table: "VarianteMateriales",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianteMateriales_VarianteId",
                table: "VarianteMateriales",
                column: "VarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianteServicios_ServicioId",
                table: "VarianteServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianteServicios_VarianteId",
                table: "VarianteServicios",
                column: "VarianteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionGlobal");

            migrationBuilder.DropTable(
                name: "HistorialPreciosIngredientes");

            migrationBuilder.DropTable(
                name: "HistorialPreciosMateriales");

            migrationBuilder.DropTable(
                name: "PresupuestoItemIngredienteDetalles");

            migrationBuilder.DropTable(
                name: "PresupuestoItemMaterialDetalles");

            migrationBuilder.DropTable(
                name: "PresupuestoItemServicioDetalles");

            migrationBuilder.DropTable(
                name: "RecetaIngredientes");

            migrationBuilder.DropTable(
                name: "RecetaServicios");

            migrationBuilder.DropTable(
                name: "VarianteIngredienteOverrides");

            migrationBuilder.DropTable(
                name: "VarianteMateriales");

            migrationBuilder.DropTable(
                name: "VarianteServicios");

            migrationBuilder.DropTable(
                name: "PresupuestoItems");

            migrationBuilder.DropTable(
                name: "Ingredientes");

            migrationBuilder.DropTable(
                name: "Materiales");

            migrationBuilder.DropTable(
                name: "ProductoVariantes");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Presupuestos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Recetas");

            migrationBuilder.DropTable(
                name: "UnidadesMedida");
        }
    }
}
