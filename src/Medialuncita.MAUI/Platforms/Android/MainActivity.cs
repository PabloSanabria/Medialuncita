using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace Medialuncita.MAUI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Android 15+ (API 35+, incluye el emulador API 36) fuerza "edge to edge" en
        // apps nuevas: el contenido se dibuja debajo de la barra de estado del sistema,
        // y el sistema intercepta los toques en esa franja superior para sus propios
        // gestos. El botón de menú del NavMenu queda posicionado casi pegado al borde
        // (`top: 0.5rem` en NavMenu.razor.css) y termina en esa franja: se ve, pero no
        // recibe el tap. Se restaura el layout clásico (contenido debajo de la barra de
        // estado, no superpuesto) hasta que la UI compartida maneje los insets de forma
        // explícita.
        WindowCompat.SetDecorFitsSystemWindows(Window!, true);
    }
}
