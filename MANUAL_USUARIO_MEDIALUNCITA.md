# Manual de usuario Medialuncita

## Propósito

Medialuncita organiza la información necesaria para conocer el costo real y el
precio de venta de productos de repostería. La aplicación se usa en orden:
primero se cargan las unidades, los ingredientes y sus precios; después se
crean las recetas, los productos y sus variantes.

Este manual describe las pantallas disponibles actualmente en la aplicación
MAUI, incluyendo el cálculo detallado de costo y precio de venta por variante.

## Antes de empezar

Para evitar errores al crear recetas y productos, cargá los datos en este
orden:

1. Unidades de medida.
2. Ingredientes y sus precios.
3. Materiales de packaging y sus precios, si ya los conocés.
4. Servicios (gas, luz, alquiler), si los vas a prorratear.
5. Configuración global (tarifa de mano de obra, estrategia de precio y
   redondeo por defecto).
6. Recetas.
7. Productos.
8. Variantes de cada producto (packaging, servicios propios y, si hace falta,
   una estrategia de precio distinta a la global).

Los precios se registran por la unidad de compra. Por ejemplo, si la harina se
compra por kilogramo, ingresá el precio de un kilogramo, aunque en la receta la
uses en gramos.

## Uso en Android

La app se maneja igual en Android que en Windows; la única diferencia es de
navegación. El menú lateral (Unidades, Ingredientes, Recetas, etc.) se abre
tocando el ícono de tres rayas (☰) arriba a la derecha de la pantalla. En
algunos equipos ese ícono puede verse pegado bien arriba, cerca del borde —
es solo estético, igual responde al toque.

## Ejemplo guiado: costear medialunas de manteca de punta a punta

Esta sección recorre TODAS las pantallas en el orden real en que se usan,
con un ejemplo numérico completo. La idea es que puedas seguirlo con
números propios la primera vez que uses la app. **Los montos exactos que te
muestre la app pueden variar levemente según redondeos internos del motor
de costeo — lo importante acá es entender el flujo, no reproducir estas
cifras al centavo.**

### 1. Unidades de medida

Creá (si no existen):

| Nombre     | Abreviatura | Tipo   | Factor a unidad base |
|------------|-------------|--------|----------------------|
| Kilogramo  | kg          | Peso   | 1000                 |
| Unidad     | u           | Unidad | 1                     |

(El factor 1000 en Kilogramo significa que 1 kg = 1000 g; la app trabaja
internamente en la unidad base — gramos para peso, mililitros para volumen.)

### 2. Ingredientes y precios

Cargá estos cuatro, todos sin merma para simplificar el ejemplo:

| Ingrediente | Unidad de compra | Precio      |
|-------------|-------------------|-------------|
| Harina 000  | Kilogramo         | $1.200/kg   |
| Manteca     | Kilogramo         | $4.000/kg   |
| Azúcar      | Kilogramo         | $1.000/kg   |
| Huevo       | Unidad            | $150 c/u    |

Para cada uno, entrá a **Ver precios** y cargá el importe con la fecha de
hoy.

### 3. Servicios

Cargá un servicio **"Gas horno"** con costo por lote: $300 (dejá vacío el
costo por hora si no lo vas a usar).

### 4. Configuración global

Cargá:
- Tarifa de mano de obra: **$3.000/hora**.
- Estrategia de precio por defecto: **Margen**, valor **40%**.
- Redondeo por defecto: al **$50** más cercano.

### 5. Receta

Creá la receta **"Medialunas de manteca"**, rendimiento **30 unidades**,
tiempo de preparación **60 minutos**. Agregá estos ingredientes:

| Ingrediente | Cantidad en la receta | Costo         |
|-------------|------------------------|---------------|
| Harina 000  | 1000 g (= 1 kg)        | $1.200        |
| Manteca     | 300 g (= 0,3 kg)       | $1.200        |
| Azúcar      | 150 g (= 0,15 kg)      | $150          |
| Huevo       | 2 unidades             | $300          |
| **Subtotal ingredientes** |          | **$2.850**    |

Desde la edición de la receta, asociá el servicio **Gas horno** con
prorrateo **por lote**.

Con esto, el costo del lote completo ya suma: $2.850 (ingredientes) + $300
(gas) + $3.000 (mano de obra, 60 min = 1 hora × $3.000/hora) = **$6.150**
para las 30 unidades del lote, es decir **$205 por unidad** antes de
packaging ni margen.

### 6. Producto

Creá el producto **"Medialuna de manteca"** usando la receta anterior como
receta madre.

### 7. Variante

Desde **Ver variantes** del producto, creá la variante **"Docena"**:
rendimiento **12 unidades**, tiempo adicional por lote **10 minutos**
(armado de la caja). En su pantalla de detalle:
- **Asignar packaging**: 1 "Caja x12" a $250 (si todavía no existe el
  material, cargalo antes desde **Materiales**).
- Dejá la estrategia de precio en "-- Usar valor global --" (hereda el
  margen 40% y el redondeo a $50 de Configuración).
- Tocá **Calcular costo y precio de venta**. El desglose muestra algo del
  estilo: 12 unidades × $205 = $2.460 de ingredientes+gas+mano de obra base,
  más $500 de mano de obra extra (10 min) y $250 de packaging → costo total
  de la docena ≈ $3.210, con un precio de venta final (margen 40%,
  redondeado a $50) de **alrededor de $4.500 la docena**.

### 8. Presupuesto

En **Presupuestos**:
1. Cargá el cliente, por ejemplo "Panadería La Espiga".
2. En **Agregar ítem**, elegí producto "Medialuna de manteca", variante
   "Docena", cantidad 3 (para cotizar 3 docenas). Se agrega la línea con su
   subtotal (≈ 3 × $4.500 = $13.500).
3. Agregá más ítems si querés (por ejemplo, otra variante o producto).
4. **Guardar presupuesto**. Se abre el detalle recién creado, con el
   snapshot congelado de todo lo calculado.

### 9. PDF

Desde el detalle del presupuesto guardado, tocá **Generar PDF**. Se abre el
cuadro nativo de "Guardar o compartir" (en Android podés mandarlo directo
por WhatsApp o guardarlo; en Windows elegís la carpeta).

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

## Servicios

En **Servicios**, cargá los costos prorrateables (gas, electricidad, agua,
alquiler, etc.). Cada servicio necesita al menos un costo: por hora, por lote,
o ambos — el modo de prorrateo se elige después, al asignarlo a una receta o
a una variante.

Si un servicio ya está asignado a alguna receta o variante, al eliminarlo se
marca como inactivo en vez de borrarlo, para no romper los cálculos existentes.

## Configuración global

En **Configuración**, definí:

- la **tarifa de mano de obra por hora**, usada para costear el tiempo de
  preparación de todas las variantes;
- la **estrategia de precio por defecto** (Margen, Multiplicador o Manual) y
  su valor asociado (margen % o multiplicador);
- la **estrategia de redondeo por defecto** del precio final.

Cualquier variante puede sobreescribir la estrategia de precio y el redondeo
desde su propia pantalla; si no lo hace, usa estos valores globales.

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

Desde la pantalla de edición de la receta también podés asociar **servicios**
que se prorratean sobre el lote completo (por ejemplo, el gas del horno). Para
cada servicio elegís si se cobra por hora o por lote; solo aparecen disponibles
los servicios que tienen cargado el costo correspondiente a ese modo.

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

Desde el producto, hacé clic en **Ver detalle / calcular** en cualquier
variante para entrar a su pantalla de detalle, donde podés:

- **Asignar packaging**: elegí un material y la cantidad que usa esta
  variante (no se recalcula automáticamente al cambiar el rendimiento — el
  packaging se declara por variante).
- **Asignar servicios propios**: además de los servicios de la receta madre,
  una variante puede tener servicios adicionales (por ejemplo, un horno extra
  para decoración).
- **Definir su propia estrategia de precio**: si dejás los campos en
  "-- Usar valor global --", la variante hereda la configuración general.
  Si elegís Margen, Multiplicador o Manual, cargá el valor correspondiente
  (margen %, multiplicador o precio fijo) y, si querés, un redondeo distinto
  al general.
- **Calcular costo y precio de venta**: el botón "Calcular costo y precio de
  venta" muestra el desglose completo — cada ingrediente con su merma y
  precio, cada material de packaging, cada servicio, el tiempo total y la
  mano de obra, el costo total del lote, el costo por unidad y el precio de
  venta final ya redondeado.

Para poder calcular, todos los ingredientes y materiales usados por la
variante (y por su receta madre) necesitan tener al menos un precio cargado
en su historial. Si falta alguno, el cálculo muestra qué precio falta en vez
de arrojar un resultado incompleto.

## Presupuestos

En **Presupuestos** vas a encontrar, arriba, la lista de presupuestos ya
guardados (fecha, cliente, cantidad de ítems y total), cada uno con un link
**Ver detalle**.

Más abajo está el formulario para armar uno nuevo:

1. Cargá, si querés, el nombre del cliente y notas (ambos opcionales).
2. En **Agregar ítem**, elegí un producto, después una de sus variantes, y
   la cantidad que querés cotizar. Al hacer clic en **Agregar ítem**, la
   pantalla calcula el precio unitario vigente con el mismo motor de costeo
   que usa "Detalle de variante" y agrega la línea a la tabla de ítems, con
   su subtotal.
3. Podés agregar tantos ítems como necesites (por ejemplo, "12 individuales"
   + "1 torta de 20 porciones" en el mismo presupuesto) y quitar cualquiera
   con el botón **Quitar** antes de guardar.
4. Cuando la lista de ítems está lista, hacé clic en **Guardar presupuesto**.
   Ahí se recalcula todo una última vez y se congela como snapshot: cada
   ítem guarda el precio unitario, el subtotal y el detalle completo del
   costeo (ingredientes, packaging, mano de obra, servicios) tal como
   estaban en ese momento. Después de guardar, se abre el detalle del
   presupuesto recién creado.

Para poder agregar un ítem, la variante elegida necesita poder costearse (los
mismos requisitos que en "Detalle de variante": todos sus ingredientes y
materiales deben tener un precio vigente cargado). Si falta algo, el error se
muestra debajo del formulario de agregar ítem.

Un presupuesto guardado **no cambia** aunque después actualices precios,
edites recetas o borres el producto/variante que le dio origen: los datos que
ves en su detalle son siempre los del momento en que se generó.

### Generar el PDF de un presupuesto

Desde el detalle de cualquier presupuesto ya guardado, el botón **Generar
PDF** arma un PDF con número de presupuesto, fecha, cliente (si lo cargaste),
la tabla de ítems (producto, variante, cantidad, precio unitario y subtotal)
y el total (el nombre de archivo incluye el número de presupuesto y la
fecha, por ejemplo `Presupuesto_12_20260910.pdf`). Al tocar el botón se abre
el cuadro nativo de "Guardar o compartir" del sistema: en Windows elegís
dónde guardarlo, y en Android además podés compartirlo directo por
WhatsApp, Drive, correo, etc.

El PDF muestra exactamente los mismos números que ya ves en pantalla: se arma
a partir del snapshot congelado, nunca vuelve a consultar precios ni a
recalcular nada. Si el presupuesto tiene muchos ítems, se generan varias
páginas automáticamente.

Por ahora esta primera versión no asocia un cliente del catálogo (todavía no
hay catálogo de clientes): el campo "Cliente" es solo un texto libre.

## Próximas pantallas

El siguiente objetivo es agregar persistencia real en la versión Web (WASM)
y, más adelante, un catálogo de clientes.

## Ayuda ante errores frecuentes

**No aparece una unidad al crear una variante.** La unidad debe ser del mismo
tipo que el rendimiento de la receta madre.

**No puedo usar gramos para un ingrediente comprado por litro.** Cargá la
densidad del ingrediente si la conversión física es válida.

**No puedo eliminar una receta o una unidad.** Está siendo usada por otra
información. Revisá los datos que la referencian antes de eliminarla.

**No hay precio vigente.** Abrí el ingrediente o material, ingresá un precio
mayor a cero y guardalo con la fecha correspondiente.

**No puedo agregar un servicio a una receta o variante.** El servicio no
tiene cargado el costo correspondiente al modo de prorrateo elegido (por
hora o por lote). Editalo desde **Servicios** y completá ese costo.

## Mantenimiento del manual

Esta copia se actualizará junto con cada funcionalidad nueva o cambio de
pantalla. Si encontrás una instrucción que no coincide con la aplicación,
registrala como incidencia antes de usar ese flujo para costos reales.
