# CLAUDE.md — Guía de contexto para trabajar en Medialuncita

Este archivo existe para que cualquier sesión de Claude (en otro chat, sin memoria
previa) pueda retomar el proyecto rápido, sin repetir exploración. Se actualiza al
final de cada entrega relevante.

## Qué es Medialuncita

App personal, offline-first, para calcular costos y generar presupuestos de
productos de repostería. Uso solo por Pablo, sin login ni sync en la nube.

## Stack

- C# / .NET 10
- .NET MAUI Blazor Hybrid (host principal, target actual: **solo Windows**,
  `net10.0-windows10.0.19041.0` — Android sigue pendiente de agregar al `.csproj`)
- Razor Class Library (`Medialuncita.UI`) compartida entre MAUI y Web
- Blazor WebAssembly (`Medialuncita.Web`) — existe como proyecto pero es un
  **placeholder sin persistencia SQLite en el navegador** todavía
- SQLite vía EF Core (`Medialuncita.Infrastructure`)
- xUnit para tests (`tests/Medialuncita.Application.Tests`)

## Arquitectura (capas, sin CQRS/MediatR/Prism/microservicios)

```
src/
  Medialuncita.Domain         Entidades y enums puros, sin dependencias
  Medialuncita.Application    Servicios de caso de uso + abstracciones (interfaces de repos)
  Medialuncita.Infrastructure EF Core, DbContext, migraciones, repos concretos
  Medialuncita.UI             Razor Class Library con TODAS las pantallas (compartidas)
  Medialuncita.MAUI           Host MAUI (Windows). Referencia Medialuncita.UI
  Medialuncita.Web            Host Blazor WASM (placeholder, sin SQLite en browser)
tests/
  Medialuncita.Application.Tests
```

Regla de oro: **las pantallas viven en `Medialuncita.UI`**, no en MAUI ni en Web.
Los hosts solo arrancan la app y hacen wiring de DI/DB.

## Pipeline de dominio

```
Ingredientes/Materiales + HistorialPrecio
        → Receta (ingredientes + merma + tiempo base + servicios)
        → ProductoVariante (rendimiento propio, overrides, packaging, servicios, tiempos extra)
        → CosteoService.CalcularCosto() → ResultadoCosteo (determinístico)
        → CosteoService.CalcularPrecioVenta() → ResultadoPrecioVenta
        → PresupuestoService → Presupuesto (snapshot congelado)
```

## Decisiones de dominio ya tomadas (no revisitar sin discutirlo)

- **Precio = historial, no campo mutable.** `HistorialPrecioIngrediente` /
  `HistorialPrecioMaterial` guardan solo `Fecha` y `Precio`; la unidad de referencia
  del precio es siempre la `UnidadCompra` de la entidad padre (ingrediente/material),
  no viaja en el historial.
- **Merma:** `cantidad_efectiva = cantidad_requerida / (1 - merma)`, aplicada ANTES
  de costear (nunca como resta posterior). Merma debe estar en `[0, 1)`.
- **ProductoVariante** usa `RendimientoCantidad` explícito + `factorEscala =
  variante.RendimientoCantidad / receta.RendimientoBaseCantidad`. Permite overrides
  puntuales por ingrediente (`VarianteIngredienteOverride`, con su propia unidad).
- **Packaging** (`VarianteMaterial`) se declara por variante, nunca se autoescala.
- **Tres componentes de tiempo, cada uno con regla propia:**
  1. `receta.TiempoPreparacionBaseMinutos * factorEscala` (prorrateado por escala)
  2. `variante.TiempoAdicionalPorLoteMinutos` (fijo, no escala por unidad)
  3. `variante.TiempoAdicionalPorUnidadMinutos * variante.RendimientoCantidad`
- **Servicios** (`Servicio`, gas/luz/alquiler) se prorratean por `ModoProrrateo`
  (`PorHora` usa horas totales del lote × `CostoPorHora`; `PorLote` usa
  `CostoPorLote` fijo). Pueden asociarse a nivel `Receta` (`RecetaServicio`) y/o a
  nivel `ProductoVariante` (`VarianteServicio`); ambos se suman.
- **Estrategia de precio** (`EstrategiaPrecio`: Margen / Multiplicador / Manual) y
  **redondeo** (`EstrategiaRedondeo`: SinRedondeo / Entero / Multiplo50 / Multiplo100)
  tienen default en `ConfiguracionGlobal` (fila única, `Id = 1`) y cada
  `ProductoVariante` puede sobreescribirlos con sus campos `*Override` (nullable =
  hereda el default global).
- **El motor de costeo es 100% determinístico y vive en código** (`CosteoService`).
  Una IA NUNCA calcula costos; a lo sumo puede *sugerir* un precio que se carga
  pasando por el flujo normal de `IPrecioConsultaService.RegistrarPrecio*Async`
  (o sea, insertando un `HistorialPrecio*`, nunca escribiendo un campo directo).
- **Al guardar la estrategia de precio de una variante**, la UI limpia los campos
  `*Override` numéricos que no corresponden a la estrategia elegida (ej: si se
  elige Margen, `MultiplicadorOverride` y `PrecioManualOverride` se guardan en
  `null`). Evita datos "fantasma" que no se usan pero podrían confundir en una
  futura edición.

## Patrón de edición de repos (importante, no reinventar)

No hay `UpdateAsync` genérico. Las pantallas de edición:
1. Cargan la entidad vía `GetByIdAsync` (instancia trackeada por el `DbContext`,
   que vive toda la sesión de la app — no hay attach/detach manual).
2. Mutan sus propiedades directamente en el objeto en memoria.
3. Llaman a `IUnitOfWork.SaveChangesAsync()`.

Repos "finos a propósito": cada interfaz en `Abstractions/IRepositories.cs` expone
solo lo que el caso de uso necesita, sin genéricos mágicos ni Specification pattern.

Borrado: catálogo (Ingrediente, Material) usa **soft-delete** (`Activo = false`)
si está en uso (`ContarRecetasQueLoUsanAsync` / `ContarVariantesQueLoUsanAsync` > 0),
si no se borra físico. Receta/Producto no tienen soft-delete: se bloquea el borrado
si `ContarProductosQueLaUsanAsync` > 0 (recetas) o se borra en cascada (productos →
sus variantes). Presupuestos son snapshots inmutables: no dependen de FK viva a
receta/producto/variante, así que sobreviven a cualquier borrado.

## Estado actual del código (ver `git log` para el detalle real)

Hecho:
- Domain completo: Ingrediente, Material, HistorialPrecio(Ingrediente/Material),
  UnidadMedida, Receta + RecetaIngrediente + RecetaServicio, Producto +
  ProductoVariante + VarianteIngredienteOverride + VarianteMaterial +
  VarianteServicio, Servicio, ConfiguracionGlobal, Presupuesto (snapshot).
- `CosteoService` (Application/Costeo): calcula ingredientes, materiales/packaging,
  mano de obra (3 componentes de tiempo), servicios (por hora/por lote), costo
  total, costo unitario, y `CalcularPrecioVenta` con las 3 estrategias + redondeo.
- `PrecioConsultaService`: precio vigente + historial + alta/baja de precios.
- `PresupuestoService`: congela un cálculo en snapshot auditable.
- `IProductoRepository.GetVarianteParaCosteoAsync`: ya trae el grafo completo
  (variante → producto → receta madre con ingredientes/servicios, + overrides,
  materiales y servicios propios de la variante) listo para pasarle a
  `CosteoService.CalcularCosto`. Es el método que alimenta tanto
  `PresupuestoService` como la pantalla `VarianteDetalle.razor`.
- `IServicioRepository` expone `ContarUsosAsync` (RecetaServicio + VarianteServicio
  distintos) para decidir soft-delete vs. borrado físico, igual que Ingrediente/Material.
- UI (`Medialuncita.UI/Components`) con CRUD funcional para:
  - `Unidades/` — unidades de medida
  - `Ingredientes/` — ingredientes + historial de precios
  - `Materiales/` — materiales/packaging + historial de precios
  - `Servicios/` — servicios prorrateables (costo por hora y/o por lote),
    con soft-delete si están en uso
  - `Recetas/` — recetas con sus líneas de ingrediente, merma, y sus
    `RecetaServicio` asociados (prorrateados sobre el lote completo)
  - `Productos/` — productos y variantes (alta/baja/edición básica), y
    **`VarianteDetalle.razor`**: pantalla central de la variante con
    packaging (`VarianteMaterial`), servicios propios (`VarianteServicio`),
    estrategia de precio/redondeo (override o heredado de `ConfiguracionGlobal`),
    y el botón "Calcular costo y precio de venta" que invoca `CosteoService`
    y `PrecioConsultaService` en vivo y muestra el desglose completo.
  - `Configuracion/ConfiguracionEditar.razor` — tarifa de mano de obra,
    estrategia de precio/redondeo por defecto (fila única `ConfiguracionGlobal`).
- 47 tests pasando (`dotnet test Medialuncita.sln`): conversión de unidades,
  costeo, precio de venta, integración de presupuesto con SQLite in-memory real,
  CRUD/soft-delete/historial de precios para las 4 entidades de catálogo + Servicio,
  y un test de integración end-to-end (`VarianteConPackagingYServiciosIntegrationTests`)
  que ejercita `GetVarianteParaCosteoAsync` + `CosteoService` con packaging,
  servicios de receta Y de variante, y override de estrategia de precio al mismo
  tiempo — el mismo camino que usa `VarianteDetalle.razor`.

Falta (= "Próximo alcance del MVP" del README, pendiente de priorizar en cada
sesión):
1. Generar presupuestos (cotizaciones) desde la UI — hoy `PresupuestoService`
   solo se ejerce desde tests, no hay pantalla que llame a
   `GenerarPresupuestoAsync` ni que liste/muestre presupuestos ya generados
   (aunque `IPresupuestoRepository.GetAllAsync`/`GetByIdAsync` ya existen).
2. Persistencia SQLite real en `Medialuncita.Web` (WASM) — hoy es placeholder.
3. Agregar `TargetFramework` de Android al `.csproj` de MAUI.
4. Editar los `VarianteIngredienteOverride` desde una UI dedicada (hoy el
   modelo los soporta y `CosteoService`/`GetVarianteParaCosteoAsync` ya los
   contemplan, pero no hay pantalla para cargarlos).

## Entrega de validación y cierre (revisión estática post-MVP)

Sesión dedicada exclusivamente a verificar de punta a punta el flujo:
Ingrediente+precio → Receta → Producto → Variante → Packaging/Servicios →
Configuración global → Cálculo detallado → Costo total/unitario → Precio de
venta. No se agregó funcionalidad nueva (presupuestos, PDF e IA siguen fuera
de alcance).

**Método usado:** revisión de código estática, archivo por archivo, en vez de
ejecución real. El sandbox de esta sesión no tiene acceso a `nuget.org`
(bloqueado por la config de red: `x-deny-reason: host_not_allowed`), así que
no se pudieron restaurar paquetes NuGet ni correr `dotnet build`/`dotnet test`
para `Application`/`Infrastructure`/`UI`/`Web`/`Tests` (sí compiló `Domain`,
que no tiene dependencias externas). `MAUI` tampoco compila en Linux por el
workload de Windows, como ya se sabía. **Pablo debe correr `dotnet build` y
`dotnet test` en su VS y confirmar el resultado** — esta sesión no lo
certificó por ejecución real.

**Verificado por lectura de código (sin encontrar problemas):**
- `CosteoService` es una única implementación (`Scoped` en DI), usada
  idénticamente por `VarianteDetalle.razor` y por
  `VarianteConPackagingYServiciosIntegrationTests`. `GetVarianteParaCosteoAsync`
  trae exactamente el grafo que el servicio necesita.
- Ingredientes (con merma y overrides), packaging, mano de obra (3
  componentes de tiempo) y servicios (por hora/por lote, de receta y de
  variante) se calculan y suman correctamente.
- `VarianteDetalle.CalcularAsync` aplica bien el patrón override-o-default
  de `ConfiguracionGlobal` para estrategia, margen, multiplicador y redondeo.
- Barrido completo de `Medialuncita.UI/Components`: ninguna pantalla con
  `InputText`/`InputNumber`/`InputSelect`/`InputDate` fuera de un `EditForm`,
  y ningún `@bind-Value` a un indexador de diccionario/lista. Los dos bugs
  del commit anterior no se reintrodujeron en ninguna pantalla (se revisaron
  las 19 pantallas de `Medialuncita.UI`, incluyendo `ServicioEditar`,
  `MaterialEditar`, `UnidadEditar` y `RecetaDetalle`, que faltaban del
  repaso anterior).
- Interfaces de `Abstractions/IRepositories.cs` cruzadas contra sus
  implementaciones concretas en `Infrastructure/Repositories/*.cs`: firmas
  coinciden, sin métodos faltantes.
- `MedialuncitaDbContext.OnModelCreating` consistente con las relaciones que
  usa `CosteoService`/`GetVarianteParaCosteoAsync`.

**Problema concreto encontrado y corregido:**
- `Medialuncita.MAUI/Components/Layout/NavMenu.razor` tenía dos `NavLink`
  muertos (`counter`, `weather`) apuntando a páginas de ejemplo que ya se
  habían borrado en el commit "Se quitan pantallas de ejemplo innecesarias".
  Clickearlos llevaba a `NotFoundPage`. Se quitaron esas dos entradas; el
  resto del menú (Unidades, Ingredientes, Materiales, Recetas, Productos,
  Servicios, Configuración) ya estaba completo y correcto.

## Bugs ya resueltos (no reintroducir)

- **Los componentes `InputText`/`InputNumber`/`InputSelect` de Blazor SIEMPRE deben
  estar dentro de un `<EditForm Model="...">`** (o con `EditContext` provisto).
  Sin eso, tiran `InvalidOperationException` en tiempo de ejecución apenas se
  renderizan ("requires a cascading parameter of type EditContext"), lo que en
  MAUI Blazor Hybrid se percibe como que "la app se tilda" sin mensaje claro.
  Esto pasó en `RecetasIndex.razor` y `RecetaEditar.razor` (código de Fase 1,
  nunca se había probado a mano esa pantalla) y se reintrodujo en las secciones
  de packaging/servicios de `VarianteDetalle.razor` durante esta entrega — ya
  está corregido en ambos casos envolviendo cada bloque de inputs en su propio
  `<EditForm Model="this">` (no hace falta `OnValidSubmit` si el botón ya es
  `type="button"` con su propio `@onclick`). Antes de dar por buena una pantalla
  nueva con inputs, verificar que esté envuelta en `EditForm`.
- **Nunca bindear `@bind-Value` a un indexador de diccionario/lista** (ej.
  `_diccionario[clave]`). Blazor arma el `FieldIdentifier` casteando el cuerpo
  de la expresión a `MemberExpression`; un indexador compila a una llamada a
  método (`get_Item`/`set_Item`), y ese cast tira `InvalidCastException` en
  tiempo de ejecución apenas se renderiza el input. Pasó en
  `RecetaEditar.razor` (la merma override por línea de ingrediente usaba
  `Dictionary<RecetaIngrediente, decimal?>` indexado por referencia). Se
  arregló envolviendo cada línea en una clase liviana (`LineaIngredienteEdit`)
  con una propiedad simple (`MermaOverridePorcentaje`) para bindear en vez del
  indexador. Si se necesita estado adicional por fila de una colección en una
  pantalla de edición, usar siempre una clase/record wrapper con propiedades,
  nunca un diccionario indexado directamente en el markup.
- `PresupuestoRepository`/`AgregarPrecioAsync` devolviendo `Id` ANTES de
  `SaveChangesAsync` (SQLite autoincremental asigna el Id recién al guardar) →
  siempre leer `.Id` después de `SaveChangesAsync`.
- Seed de `ConfiguracionGlobal` vía `HasData` no se aplicaba bajo
  `EnsureCreated()` en tests → el repo tiene fallback para crear la fila si no
  existe.
- Carpeta `.vs` no debe versionarse (ya está en `.gitignore` y se sacó del
  historial con `git rm --cached`).

## Convenciones de trabajo con Claude en este proyecto

- **Idioma:** todo el código, comentarios y nombres de entidades/propiedades en
  **español** (así está el codebase existente — mantener consistencia).
- **Scoping protocol:** Claude propone alcance concreto antes de escribir código;
  Pablo aprueba (o ajusta) antes de que empiece la implementación.
- **Entrega:** Pablo usa el plan gratis de Claude (sin ejecución de código ni
  acceso directo a GitHub desde su lado). El flujo es:
  1. Claude escribe los archivos en su sandbox.
  2. Claude empaqueta todo en un ZIP descargable con instrucciones de dónde
     pegar cada archivo (o crea diffs claros si son pocos archivos).
  3. Pablo aplica los cambios manualmente en Visual Studio, compila y corre los
     tests.
  4. Pablo reporta resultados (build/test) de vuelta a Claude para iterar.
- **No commitear nada directamente**: Claude no tiene push access; todo pasa por
  Pablo aplicando el ZIP y haciendo su propio commit.
- **Entorno de Pablo:** Visual Studio 2026 en Windows (requerido para soporte de
  MAUI con .NET 10).
- **Actualizar este archivo** al cierre de cada entrega con: qué se agregó, qué
  quedó pendiente, y cualquier decisión de diseño nueva que futuras sesiones deban
  respetar.

## Comandos útiles

```powershell
dotnet build Medialuncita.sln
dotnet test Medialuncita.sln
```

MAUI aplica migraciones pendientes al iniciar y crea la base SQLite en el
directorio privado de datos de la app (no hace falta correr `dotnet ef` a mano
para uso normal).
