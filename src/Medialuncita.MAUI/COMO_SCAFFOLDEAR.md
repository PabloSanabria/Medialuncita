# Medialuncita.MAUI — pendiente de scaffolding

Este proyecto **no viene con un .csproj escrito a mano** como el resto de la solución.

Motivo: un proyecto .NET MAUI Blazor Hybrid tiene mucho contenido específico de
plataforma (manifiestos de Android, Package.appxmanifest de Windows, íconos,
splash screens, TargetFrameworks múltiples) que la plantilla oficial genera
correctamente y que es fácil de romper si se escribe a mano sin poder compilarlo
para verificar. Por eso te conviene generarlo con el `dotnet` CLI real en tu máquina.

## Pasos para crear el proyecto

Desde la carpeta `src/`, con el SDK de .NET 10 y el workload de MAUI instalados:

```bash
dotnet workload install maui

dotnet new maui-blazor -n Medialuncita.MAUI
```

Esto crea `src/Medialuncita.MAUI/` con la estructura estándar (Platforms/Android,
Platforms/Windows, MauiProgram.cs, App.xaml, MainPage.xaml, Resources/, etc.).

## Ajustes a hacer sobre el proyecto generado

1. **Agregar las referencias a los proyectos de la solución**, en el `.csproj` generado:

```xml
<ItemGroup>
  <ProjectReference Include="..\Medialuncita.UI\Medialuncita.UI.csproj" />
  <ProjectReference Include="..\Medialuncita.Application\Medialuncita.Application.csproj" />
  <ProjectReference Include="..\Medialuncita.Infrastructure\Medialuncita.Infrastructure.csproj" />
</ItemGroup>
```

2. **Reemplazar el contenido de `MauiProgram.cs`** por el de `MauiProgram.reference.cs`
   (en esta misma carpeta), que ya deja registrado `AddApplication()` +
   `AddInfrastructure(...)` apuntando a un archivo SQLite en `FileSystem.AppDataDirectory`
   (funciona igual en Android y Windows sin código específico de plataforma).

3. **Agregar el proyecto a la solución**:

```bash
dotnet sln ../../Medialuncita.sln add Medialuncita.MAUI/Medialuncita.MAUI.csproj
```

4. Borrar los componentes Razor de ejemplo que trae la plantilla (`Counter.razor`,
   `Weather.razor`, etc.) si no los vas a usar — no son parte del dominio de Medialuncita.

## Sobre compilar el target Windows

El target `net10.0-windows10.0.xxxxx.0` **solo compila en Windows** (WinUI 3 es una
limitación de la plataforma, no de este proyecto). En Linux/Mac vas a poder compilar
igual el resto de la solución (`dotnet build` sobre los demás proyectos), pero para
verificar el head de Windows necesitás hacerlo desde una máquina Windows o un runner
de CI con Windows.

El target Android sí compila en Linux, pero requiere el workload + Android
SDK/NDK instalados (`dotnet workload install maui-android` alcanza si solo te
interesa Android).
