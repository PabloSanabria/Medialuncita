# Medialuncita

Aplicación .NET 10 para calcular costos y precios de productos de repostería.
El host principal es MAUI para Windows y Android, con SQLite local y
funcionamiento offline.

## Estado actual

La solución contiene Domain, Application, Infrastructure, UI compartida,
MAUI, Web WASM y tests. Compila con .NET 10 y la suite automatizada tiene 52
tests aprobados.

La UI de MAUI actualmente permite:

- administrar unidades de medida;
- administrar ingredientes y su historial de precios;
- administrar materiales/packaging y su historial de precios;
- administrar servicios prorrateables (gas, luz, alquiler, etc.);
- crear, consultar, editar y eliminar recetas con sus ingredientes, mermas y
  servicios asociados;
- crear, consultar, editar y eliminar productos y sus variantes;
- asignar packaging y servicios propios a cada variante;
- definir la estrategia de precio y redondeo de cada variante (o heredar la
  configuración global);
- editar la configuración global (tarifa de mano de obra, estrategia de
  precio/redondeo por defecto);
- **calcular el costo y precio de venta detallado de una variante desde la
  pantalla de la variante**, mostrando el desglose de ingredientes, packaging,
  mano de obra y servicios;
- **generar presupuestos (cotizaciones) básicos desde la pantalla
  "Presupuestos"**: se arma agregando una o más líneas de Producto + Variante
  + Cantidad, se previsualiza el precio unitario/subtotal con el mismo motor
  de costeo, y al guardar se congela todo en un snapshot inmutable; también
  se puede consultar cualquier presupuesto ya guardado;
- **exportar a PDF cualquier presupuesto ya guardado**, con el botón
  "Generar PDF" en el detalle del presupuesto. El PDF se arma exclusivamente
  con el snapshot congelado (`PresupuestoPdfService`, en `Application`): no
  vuelve a consultar precios ni a recalcular nada, así que siempre coincide
  con lo que se ve en pantalla. Pagina automáticamente si el presupuesto
  tiene muchos ítems. En MAUI (Windows y Android) el archivo se entrega a
  través del share sheet nativo (`IArchivoDescargaService` /
  `ArchivoDescargaServiceMaui`), no de una descarga de navegador.

Cada producto referencia una receta madre. Cada variante define su rendimiento,
unidad compatible y los tiempos adicionales por lote y por unidad. Al eliminar
un producto se eliminan sus variantes; los presupuestos existentes conservan
sus datos históricos porque almacenan snapshots y no una clave foránea.

## Modelo de costeo disponible

El núcleo ya calcula, de manera determinística:

1. ingredientes y precios vigentes por historial;
2. conversiones de unidades y densidad peso/volumen;
3. merma;
4. packaging por variante;
5. mano de obra;
6. servicios por hora o por lote;
7. costo total, costo unitario y precio de venta.

Este cálculo ya es accesible desde la interfaz, tanto en la pantalla "Detalle
de variante" (cálculo puntual) como en "Presupuestos" (cálculo + snapshot
persistente vía `PresupuestoService`, que congela el resultado en un
snapshot auditable).

## Próximo alcance del MVP

- Persistencia SQLite real en `Medialuncita.Web` (WASM) — hoy es un placeholder.
- Gestión de clientes (fuera de alcance de esta entrega).
- Editar los `VarianteIngredienteOverride` desde una UI dedicada.
- Publicación en Google Play (fuera de alcance; el APK actual es solo para
  instalación directa/sideload).

## Ejecutar

Desde la raíz de la solución:

```powershell
dotnet build Medialuncita.sln
dotnet test Medialuncita.sln
```

MAUI aplica las migraciones pendientes al iniciar y crea la base SQLite en el
directorio privado de datos de la aplicación.

## Android

Requiere Visual Studio 2026 con el workload ".NET Multi-platform App UI
development" y la plataforma Android instalada (Herramientas → Obtener
herramientas y características → workload MAUI, sub-ítem Android SDK).

**Compilar/ejecutar en un emulador o dispositivo (Debug):**
1. Abrir `Medialuncita.sln` en Visual Studio 2026.
2. En la barra de "Debug Target", elegir `Medialuncita.MAUI` con el framework
   `net10.0-android` y un emulador o dispositivo Android conectado (con
   depuración USB habilitada).
3. F5. La primera vez, Visual Studio descarga/instala el emulador o pide
   habilitar la depuración en el dispositivo físico.

**Generar el APK Release instalable (sideload, sin Google Play):**
1. Cambiar la configuración de solución a `Release`.
2. Clic derecho sobre `Medialuncita.MAUI` → **Publicar**.
3. Elegir **Ad Hoc** (o "Carpeta"/sideload) como método de distribución para
   Android. Visual Studio genera automáticamente un keystore de firma si no
   hay uno configurado (ver `AndroidKeyStore` en el `.csproj`: queda en
   `false` para Release, que habilita esta firma automática en vez de pedir
   un keystore propio).
4. Publicar. El `.apk` resultante queda bajo
   `src/Medialuncita.MAUI/bin/Release/net10.0-android/publish/`.
5. Copiar el `.apk` al dispositivo (cable, Drive, etc.) e instalarlo
   habilitando antes "Instalar apps de orígenes desconocidos" en Android.

Notas:
- El target framework Android es `net10.0-android`, con
  `RuntimeIdentifiers` limitado a `android-arm64;android-arm` (cubre los
  dispositivos reales; se excluyen `x86`/`x64`, que solo existen como
  emuladores).
- `TrimMode` se fija en `Partial` para Android (ver comentario en el
  `.csproj`): el modo `Full` (default de Release) recorta por reflexión
  miembros que EF Core necesita en runtime y puede romper el arranque de la
  app en el dispositivo aunque la compilación no muestre errores.
- Si al ejecutar en un dispositivo aparece `DllNotFoundException` mencionando
  `e_sqlite3`, agregar una referencia explícita a
  `SQLitePCLRaw.bundle_e_sqlite3` en `Medialuncita.MAUI.csproj` (no se agregó
  preventivamente en esta entrega para no fijar una versión sin poder
  validarla; ver `CLAUDE.md`).
