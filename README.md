# Medialuncita

Aplicación .NET 10 para calcular costos y precios de productos de repostería.
El host principal es MAUI para Windows, con SQLite local y funcionamiento
offline.

## Estado actual

La solución contiene Domain, Application, Infrastructure, UI compartida,
MAUI, Web WASM y tests. Compila con .NET 10 y la suite automatizada tiene 44
tests aprobados.

La UI de MAUI actualmente permite:

- administrar unidades de medida;
- administrar ingredientes y su historial de precios;
- administrar materiales/packaging y su historial de precios;
- crear, consultar, editar y eliminar recetas con sus ingredientes y mermas;
- crear, consultar, editar y eliminar productos y sus variantes.

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

También existe `PresupuestoService`, que congela el resultado de un cálculo
en un snapshot auditable.

## Próximo alcance del MVP

Faltan las pantallas para asignar packaging y servicios a variantes/recetas,
configurar mano de obra y estrategia de precio, y mostrar el cálculo detallado
de una variante desde la interfaz. Web/PWA sigue siendo un placeholder sin
persistencia SQLite en el navegador.

## Ejecutar

Desde la raíz de la solución:

```powershell
dotnet build Medialuncita.sln
dotnet test Medialuncita.sln
```

MAUI aplica las migraciones pendientes al iniciar y crea la base SQLite en el
directorio privado de datos de la aplicación.
