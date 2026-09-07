# Manual de usuario Medialuncita

## Propósito

Medialuncita organiza la información necesaria para conocer el costo real y el
precio de venta de productos de repostería. La aplicación se usa en orden:
primero se cargan las unidades, los ingredientes y sus precios; después se
crean las recetas, los productos y sus variantes.

Este manual describe las pantallas disponibles actualmente en la aplicación
MAUI. Las opciones de costeo detallado, packaging por variante, servicios,
mano de obra y precio final todavía no tienen pantalla de usuario.

## Antes de empezar

Para evitar errores al crear recetas y productos, cargá los datos en este
orden:

1. Unidades de medida.
2. Ingredientes y sus precios.
3. Materiales de packaging y sus precios, si ya los conocés.
4. Recetas.
5. Productos.
6. Variantes de cada producto.

Los precios se registran por la unidad de compra. Por ejemplo, si la harina se
compra por kilogramo, ingresá el precio de un kilogramo, aunque en la receta la
uses en gramos.

## Unidades de medida

Abrí **Unidades** desde el menú lateral. Cada unidad necesita nombre,
abreviatura, tipo y factor a unidad base. Usá 1000 para kilogramo, litro y
docena; usá 1 para gramo, mililitro y unidad.

No se puede eliminar una unidad que esté siendo utilizada por ingredientes,
materiales, recetas o variantes.

## Ingredientes y precios

En **Ingredientes**, elegí la unidad en la que comprás cada ingrediente y
cargá su merma habitual como porcentaje. Si una receta usa una medida de peso
y el ingrediente se compra por volumen, o al revés, cargá la densidad en gramos
por mililitro.

Para registrar un precio, abrí **Ver precios** desde el ingrediente. Indicá la
fecha y el importe por unidad de compra. El último precio por fecha es el
vigente. Los precios anteriores se conservan como historial; si te equivocaste,
eliminá el registro incorrecto y cargá uno nuevo.

Si un ingrediente ya se usa en una receta, al eliminarlo se marca como
inactivo para preservar las recetas existentes.

## Materiales y packaging

La pantalla **Materiales** funciona igual que Ingredientes y está pensada para
cajas, etiquetas, bandejas, film u otros insumos de empaque. Cargá la unidad
de compra, la merma y el historial de precios.

Por ahora los materiales se registran en el catálogo, pero todavía no se
asignan desde la interfaz a una variante de producto.

## Recetas

En **Recetas**, ingresá nombre, descripción opcional, rendimiento y tiempo de
preparación del lote. Luego agregá sus ingredientes indicando cantidad y
unidad.

La unidad de una receta debe ser compatible con la unidad de compra del
ingrediente: peso con peso, volumen con volumen y unidad con unidad. Peso y
volumen solo se pueden cruzar cuando el ingrediente tiene densidad cargada.

Podés dejar vacía la merma override para usar la merma habitual del
ingrediente. Si la receta necesita una merma distinta, ingresala en porcentaje
para esa línea. Las recetas pueden editarse, incluyendo agregar, cambiar o
quitar ingredientes. No se puede eliminar una receta vinculada a un producto.

## Productos

En **Productos**, creá un producto seleccionando una receta madre. El producto
representa lo que vendés; la receta contiene sus proporciones de ingredientes.

Podés editar el nombre o cambiar la receta madre. Si ya tiene variantes, la
nueva receta debe usar el mismo tipo de rendimiento que esas variantes. Al
eliminar un producto también se eliminan sus variantes.

## Variantes de producto

Entrá en **Ver variantes** desde un producto para crear presentaciones
vendibles, por ejemplo "Individual", "Caja de 6" o "Torta de 20 porciones".

Cada variante requiere nombre, rendimiento (cantidad y unidad), tiempo
adicional por lote y tiempo adicional por unidad. La unidad de rendimiento de
la variante debe tener el mismo tipo que el rendimiento de la receta madre.
Podés editar o eliminar variantes desde la misma pantalla del producto.

## Próximas pantallas

El siguiente objetivo es completar el cálculo desde la interfaz. Se agregarán:

- materiales de packaging por variante;
- servicios de receta y variante;
- configuración de mano de obra y estrategia de precio;
- resumen de ingredientes, mermas, packaging, mano de obra, servicios, costo
  total, costo por unidad y precio de venta.

## Ayuda ante errores frecuentes

**No aparece una unidad al crear una variante.** La unidad debe ser del mismo
tipo que el rendimiento de la receta madre.

**No puedo usar gramos para un ingrediente comprado por litro.** Cargá la
densidad del ingrediente si la conversión física es válida.

**No puedo eliminar una receta o una unidad.** Está siendo usada por otra
información. Revisá los datos que la referencian antes de eliminarla.

**No hay precio vigente.** Abrí el ingrediente o material, ingresá un precio
mayor a cero y guardalo con la fecha correspondiente.

## Mantenimiento del manual

Esta copia se actualizará junto con cada funcionalidad nueva o cambio de
pantalla. Si encontrás una instrucción que no coincide con la aplicación,
registrala como incidencia antes de usar ese flujo para costos reales.
