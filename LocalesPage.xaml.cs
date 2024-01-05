using SalSoplado_Tienda;
using SalSoplado_Usuario.Services;

namespace SalSoplado_Usuario;

public partial class LocalesPage : ContentPage
{
    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);
    public LocalesPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
        CargarCantidadLocales();
    }

    private async void CargarCantidadLocales()
    {
        try
        {
            var cantidadLocales = await _api.GetCantidadLocales(token);
            localesLabel.Text = $"{cantidadLocales}/3 locales"; // Aseg�rate de que localesLabel est� vinculado al Label correcto

            //Habilitar el boton de crear local mientras la cantidad sea menor a 2.
            crearLocalButton.IsEnabled = cantidadLocales < 3;
        }
        catch (Exception ex)
        {
            // Maneja la excepci�n si algo sale mal
            Console.WriteLine(ex.Message);
        }
    }

    private async void OnClickCrearLocal(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CrearLocalPage());
    }
}