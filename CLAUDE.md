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

- UI (`Medialuncita.UI/Components/Presupuestos`) con la primera versión
  funcional de Presupuestos:
  - `PresupuestosIndex.razor` (`/presupuestos`): lista los presupuestos
    guardados (`IPresupuestoRepository.GetAllAsync`) y permite armar uno
    nuevo agregando ítems (Producto + Variante + Cantidad) a una lista en
    memoria ("staging", no persistida hasta guardar). Cada ítem se
    previsualiza calculando su precio unitario con `ICosteoService`
    (mismo patrón que `VarianteDetalle.CalcularAsync`: trae la variante con
    `GetVarianteParaCosteoAsync`, resuelve precios vigentes con
    `IPrecioConsultaService`, aplica `ConfiguracionGlobal`). Al guardar,
    llama a `IPresupuestoService.GenerarPresupuestoAsync` (que vuelve a
    calcular todo server-side y es la única fuente de verdad del snapshot)
    y navega al detalle del presupuesto recién creado.
  - `PresupuestoDetalle.razor` (`/presupuestos/{Id}`): consulta un
    presupuesto guardado vía `IPresupuestoRepository.GetByIdAsync` y muestra
    sus campos `*Snapshot` (producto/variante, cantidad, precio unitario,
    subtotal, total) — sin volver a costear nada.
  - Entrada "Presupuestos" agregada a `NavMenu.razor` (MAUI), entre
    Productos y Configuración.
  - **No se tocó `CosteoService` ni `PresupuestoService`**: ya traían todo
    lo necesario (el service ya soportaba múltiples ítems por presupuesto;
    la UI solo lo expone).
  - Fuera de alcance de esta entrega (documentado también en el manual de
    usuario): exportar a PDF, catálogo de clientes, sugerencias por IA.

Falta (= "Próximo alcance del MVP" del README, pendiente de priorizar en cada
sesión):
1. Persistencia SQLite real en `Medialuncita.Web` (WASM) — hoy es placeholder.
2. Editar los `VarianteIngredienteOverride` desde una UI dedicada (hoy el
   modelo los soporta y `CosteoService`/`GetVarianteParaCosteoAsync` ya los
   contemplan, pero no hay pantalla para cargarlos).
3. Catálogo de clientes (hoy `Presupuesto.ClienteNombre` es solo texto
   libre; no hay entidad `Cliente`).
4. Publicación en Google Play (fuera de alcance actual; el target Android
   hoy solo produce un `.apk` para sideload, ver sección "Entrega: target
   Android" más abajo).

(Exportar presupuestos a PDF: **ya implementado**, ver sección "Entrega:
PDF de presupuestos" más abajo. Target Android: **ya agregado**, ver sección
"Entrega: target Android" más abajo.)

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

## Entrega: Presupuestos básicos (primera versión funcional)

Objetivo único de la sesión: pantallas de Presupuestos sobre el modelo y
`PresupuestoService` ya existentes, sin rediseñar dominio ni tocar el motor
de costeo. Se respetó el scoping acordado (ver checklist de alcance en el
pedido original de la sesión).

**Revisión previa (antes de escribir código):** se leyó este archivo, la
entidad `Presupuesto`/`PresupuestoItem` (`Domain/Entities/Presupuesto.cs`),
`PresupuestoService`, `IPresupuestoRepository`/`PresupuestoRepository`, y el
test de integración existente (`PresupuestoServiceIntegrationTests`). Todo
el backend (dominio, servicio, repositorio, DI) ya estaba completo y
probado; solo faltaba la UI.

**Build/test de esta sesión:** el sandbox de esta sesión **no tiene el SDK
de .NET instalado** (`dotnet` no existe como comando), a diferencia de la
sesión anterior donde al menos `Domain` había compilado. No se pudo ejecutar
ni `dotnet build` ni `dotnet test` de ninguna forma. **Pablo debe compilar y
correr `dotnet test Medialuncita.sln` en Visual Studio** antes de dar por
buena esta entrega — la revisión de esta sesión fue 100% por lectura de
código, cruzando firmas de `ICosteoService`/`IPresupuestoService` y
propiedades de las entidades (`ProductoVariante`, `VarianteIngredienteOverride`,
`VarianteMaterial`) contra lo que usan las pantallas nuevas.

**Decisión de diseño de esta entrega:** el cálculo de precio unitario que se
muestra al agregar un ítem en `PresupuestosIndex.razor` es una
*previsualización*: duplica (no reutiliza vía llamada directa) los mismos
pasos que ya hace `PresupuestoService.GenerarPresupuestoAsync` internamente,
porque ese método es privado en su construcción del snapshot y arma+persiste
en el mismo paso. Se aceptó esta pequeña duplicación (mismo patrón que ya
usa `VarianteDetalle.CalcularAsync`) en vez de modificar `PresupuestoService`
para exponer un modo "solo cotizar sin guardar", que hubiera sido un cambio
de alcance mayor al pedido. El guardado real siempre vuelve a calcular todo
del lado del servicio, que es la única fuente de verdad del snapshot.

## Entrega: PDF de presupuestos

Objetivo único de la sesión: exportar a PDF un presupuesto **ya guardado**,
usando exclusivamente su snapshot. Sin recalcular costos/precios, sin
cliente/IA/nube/Web nuevos, sin entidades nuevas.

**Revisión previa (antes de escribir código):** se clonó el repo (solo
lectura) y se revisaron `Presupuesto`/`PresupuestoItem`
(`Domain/Entities/Presupuesto.cs`), `PresupuestoService` (no se tocó),
`IPresupuestoRepository`/`PresupuestoRepository` (`GetByIdAsync` ya trae
todo el grafo necesario vía `Include`) y se buscó infraestructura de PDF
previa en todo el repo (`find -iname "*pdf*"`): **no existía ninguna**, ni
carpeta ni paquete NuGet referenciado en ningún `.csproj`. Se implementó
desde cero.

**Decisión de diseño — generador de PDF sin dependencias NuGet:** en vez de
agregar una librería de terceros (QuestPDF, PdfSharp, etc.), se escribió un
escritor de PDF 1.4 minimalista a mano
(`Medialuncita.Application/Presupuestos/Pdf/MinimalPdfDocument.cs`): objetos
PDF planos sin comprimir, fuentes estándar Helvetica/Helvetica-Bold (no
requieren embeber datos de fuente), texto codificado en Latin-1/WinAnsi
(cubre tildes y ñ). Motivos: (1) el sandbox de Claude no tiene acceso a
`nuget.org` (ver sección de validación anterior) y no se podía verificar que
un paquete nuevo resuelva; (2) mantiene el principio de "sin abstracciones
innecesarias" — para un documento de una sola tabla no hace falta un motor
de layout completo; (3) cero dependencias nuevas = cero riesgo de romper el
offline-first. Si en el futuro se necesita un PDF visualmente más rico
(logo, múltiples estilos, gráficos), ahí sí conviene evaluar una librería
real — pero para esta entrega alcanza y sobra.

**Dónde vive cada pieza:**
- `Medialuncita.Application/Presupuestos/Pdf/MinimalPdfDocument.cs`: escritor
  de PDF genérico (texto posicionado, líneas, paginado A4). No sabe nada de
  `Presupuesto`.
- `Medialuncita.Application/Presupuestos/Pdf/IPresupuestoPdfService.cs` +
  `PresupuestoPdfService.cs`: arma el layout específico del presupuesto
  (encabezado, tabla de ítems, total, paginación automática si no entran
  todos los ítems en una página) leyendo **solo** campos `*Snapshot` /
  `PrecioUnitarioAlMomento` / `Subtotal` / `Total` ya persistidos. No
  referencia `ICosteoService`, `IPrecioConsultaService` ni ningún
  repositorio: estructuralmente no puede recalcular nada. Registrado en
  `Medialuncita.Application/DependencyInjection.cs` como
  `IPresupuestoPdfService` (`Scoped`, mismo patrón que
  `IPresupuestoService`).
- `Medialuncita.UI/Components/Presupuestos/PresupuestoDetalle.razor`: botón
  "Generar PDF" que llama a `IPresupuestoPdfService.GenerarPdf` sobre el
  `_presupuesto` que la pantalla ya tiene cargado y entrega los bytes vía
  `IArchivoDescargaService.DescargarAsync` (interfaz agregada en la entrega
  de target Android, ver sección "Entrega: target Android" más abajo — en
  esa sección está la implementación actual del mecanismo de entrega, que
  reemplazó al JS interop directo que había acá originalmente).

**Tests agregados** (`tests/Medialuncita.Application.Tests/PresupuestoPdfServiceTests.cs`,
5 tests, sin mocks — el servicio no tiene dependencias): estructura PDF
válida (`%PDF-1.4` ... `%%EOF`), que el contenido refleje EXACTAMENTE los
valores del snapshot (no otros recalculados), presupuesto sin cliente/notas,
paginación automática con muchos ítems, y escapado correcto de nombres con
paréntesis/barras.

**Build/test de esta sesión:** igual que en entregas anteriores, el sandbox
de Claude no tiene el SDK de .NET instalado ni acceso a `nuget.org`. No se
pudo correr `dotnet build`/`dotnet test`. **Pablo debe compilar y correr
`dotnet test Medialuncita.sln` en Visual Studio** antes de dar por buena
esta entrega. La verificación de esta sesión fue por lectura de código y
por trazar a mano la aritmética de paginación (altura de encabezado vs.
espacio disponible por página) para confirmar que coincide con los
desplazamientos verticales realmente dibujados.

**Fuera de alcance de esta entrega (sin cambios):** catálogo de clientes,
funcionalidad de IA, persistencia Web/PWA, nuevas entidades de dominio.

## Entrega: target Android

Objetivo único de la sesión: agregar el target Android al proyecto MAUI y
dejar preparado todo lo necesario para generar un `.apk` Release instalable
por sideload. Sin clientes, sin SQLite en Web/WASM, sin
`VarianteIngredienteOverride`, sin IA, sin nube, sin Google Play, sin
funcionalidad nueva.

**Revisión previa (antes de escribir código):** se clonó el repo (solo
lectura) y se revisó `Medialuncita.MAUI.csproj` (target único
`net10.0-windows10.0.19041.0`, pero con los `Condition` de
`SupportedOSPlatformVersion` para `android`/`ios`/`maccatalyst` ya
preparados por el scaffold original de `dotnet new maui-blazor` y sin usar),
`MauiProgram.cs` (ya usa `FileSystem.AppDataDirectory` para la ruta de la
base SQLite — portable sin cambios entre Windows/Android) y
`Platforms/Android/*` (`MainActivity.cs`, `MainApplication.cs`,
`AndroidManifest.xml` — ya generados por el scaffold, nunca compilados
porque Android no estaba en `TargetFrameworks`). No se encontró código
Windows-específico (`OSVersion`, `RuntimeInformation`, rutas con `C:\`,
`Microsoft.Win32`, etc.) en Domain/Application/Infrastructure/UI que pudiera
romper en Android.

**Cambios de configuración (`Medialuncita.MAUI.csproj`):**
- `net10.0-android` agregado a `<TargetFrameworks>` (queda
  multi-target junto con Windows).
- `RuntimeIdentifiers` = `android-arm64;android-arm` (dispositivos reales;
  se excluyen `x86`/`x64`, que solo existen como emuladores, para no inflar
  el APK).
- `AndroidPackageFormat` = `apk` explícito (no `aab` de Play Store, que está
  fuera de alcance).
- `TrimMode` = `Partial` para Android. **Esto es el problema concreto más
  importante que se previno en esta entrega:** el default de Release en
  Android es `TrimMode=Full`, que recorta por reflexión miembros que EF Core
  necesita para armar su modelo y ejecutar migraciones en runtime. Con
  `Full` la app puede compilar y empaquetar sin errores y aun así fallar o
  crashear al abrir la pantalla que toca la base de datos en el dispositivo
  real. `Partial` solo recorta ensamblados del framework marcados
  `[Trimmable]` y deja intactos EF Core/Sqlite y el código propio.
- `AndroidKeyStore=false` en Release: habilita la firma ad-hoc automática
  (keystore de debug autogenerado) también en builds Release, suficiente
  para un `.apk` instalable fuera de Play Store sin que Pablo tenga que
  crear y gestionar un keystore propio.

**Problema concreto encontrado y corregido — descarga del PDF no funciona en
el WebView de Android:** el mecanismo existente
(`medialuncita.descargarArchivo` en `archivos.js`, un `Blob` +
`<a download>` simulado por click) funciona en Windows porque WebView2
delega la descarga en el shell de Windows, pero el WebView del sistema en
Android no tiene ningún gestor de descargas enganchado por default: el click
se ejecuta pero no pasa nada, sin error visible — el botón "Generar PDF"
hubiera parecido funcionar sin producir ningún archivo accesible para el
usuario. Se resolvió con una abstracción por host, sin cambiar la lógica de
generación del PDF (`PresupuestoPdfService` no se tocó):
- `Medialuncita.UI/Services/IArchivoDescargaService.cs` (interfaz nueva,
  `DescargarAsync(nombreArchivo, contenido, tipoMime)`).
  `PresupuestoDetalle.razor` ahora inyecta esta interfaz en vez de
  `IJSRuntime` directamente.
- `Medialuncita.MAUI/Services/ArchivoDescargaServiceMaui.cs`: implementación
  única para Windows **y** Android (sin `#if` ni carpetas `Platforms/*`):
  escribe el archivo en `FileSystem.CacheDirectory` y dispara
  `Share.Default.RequestAsync` (share sheet nativo de MAUI Essentials, ya
  incluido en `Microsoft.Maui.Controls`, sin paquetes NuGet nuevos). En
  Android esto abre el selector nativo de "Guardar en Archivos / compartir
  por WhatsApp, Drive, etc."; en Windows abre el diálogo equivalente.
  Registrada en `MauiProgram.cs` (`AddTransient`).
- `Medialuncita.Web/Services/ArchivoDescargaServiceWeb.cs`: implementación
  para Web que reutiliza el JS existente sin modificarlo (en un navegador
  real el patrón Blob+`<a download>` sí funciona). Registrada en
  `Medialuncita.Web/Program.cs` (`AddScoped`) por paridad, aunque el host
  Web sigue sin SQLite real y la pantalla de Presupuestos no es funcional
  ahí de todas formas.
- `archivos.js` y su referencia en `index.html` de MAUI se dejaron
  intactos (sin uso desde MAUI ahora, pero removerlos no aportaba nada y
  sumaba riesgo innecesario a la entrega).

**Riesgo identificado y documentado, no resuelto en esta entrega —
`SQLitePCLRaw` en Android:** `Microsoft.EntityFrameworkCore.Sqlite` trae
transitivamente `SQLitePCLRaw.bundle_e_sqlite3`, que incluye los binarios
nativos de SQLite para Android. En la gran mayoría de apps MAUI esto se
resuelve solo (la selección de binario nativo la hace NuGet contra el
`TargetFramework`/RID final del proyecto ejecutable, no contra el proyecto
`Infrastructure` que lo referencia). No se agregó una referencia explícita a
`SQLitePCLRaw.bundle_e_sqlite3` en `Medialuncita.MAUI.csproj` porque el
sandbox de esta sesión no tiene acceso a `nuget.org` (ver secciones
anteriores) y fijar una versión sin poder validar que resuelve y es
compatible con `Microsoft.Data.Sqlite 10.0.0` era más riesgo que beneficio.
**Si al correr en un dispositivo/emulador Android aparece
`DllNotFoundException` mencionando `e_sqlite3`**, el fix documentado es
agregar esa `PackageReference` directamente en `Medialuncita.MAUI.csproj`
(dejar que VS resuelva la versión compatible automáticamente al agregarla
desde el NuGet Package Manager).

**Build/test/compilación Android de esta sesión:** el sandbox de Claude no
tiene el SDK de .NET instalado, no tiene acceso a `nuget.org` y, aunque los
tuviera, no tiene el workload Android de MAUI (requiere Android SDK/NDK,
que no está disponible acá). No se pudo ejecutar `dotnet build`,
`dotnet test` ni compilar el target `net10.0-android` de ninguna forma. La
verificación de esta entrega fue **100% por lectura de código y
configuración**, no por build real. **Pablo debe, en Visual Studio 2026 con
el workload Android instalado:**
1. Restaurar (`dotnet restore` o simplemente abrir en VS).
2. `dotnet build Medialuncita.sln` (compilación Windows+tests, sin tocar
   Android) y `dotnet test Medialuncita.sln`.
3. Seleccionar el framework `net10.0-android` de `Medialuncita.MAUI` como
   destino de depuración y confirmar que compila y arranca en un emulador o
   dispositivo.
4. Navegar todas las pantallas (catálogos, variante, presupuestos) y
   confirmar que el flujo de alta/edición y el cálculo de costeo funcionan
   igual que en Windows.
5. Probar específicamente "Generar PDF" desde un presupuesto guardado en
   Android y confirmar que el share sheet nativo aparece y el archivo se
   puede guardar/abrir.
6. Publicar en Release (ver README, sección "Android") y confirmar que el
   `.apk` generado instala y arranca en un dispositivo real.
Reportar cualquier error de esos pasos para poder corregirlo en la próxima
entrega.

**Fuera de alcance de esta entrega (sin cambios):** catálogo de clientes,
`VarianteIngredienteOverride`, IA, nube, publicación en Google Play (el
`.apk` generado es solo para sideload), cualquier funcionalidad nueva más
allá de lo necesario para que Android compile y funcione.

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
