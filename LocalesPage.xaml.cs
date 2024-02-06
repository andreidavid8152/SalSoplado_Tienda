using Microsoft.Maui.Media;
using Newtonsoft.Json;
using SalSoplado_Tienda;
using SalSoplado_Tienda.Models;
using SalSoplado_Usuario.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SalSoplado_Usuario;

public partial class LocalesPage : ContentPage
{
    private readonly APIService _api;
    private ObservableCollection<LocalLoad> Locales { get; set; }
    private string token = Preferences.Get("UserToken", string.Empty);
    public LocalesPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
        Locales = new ObservableCollection<LocalLoad>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarCantidadLocales(); // Llama a este método para actualizar la información cada vez que la página aparezca
        CargarLocales();
    }

    private async void CargarCantidadLocales()
    {
        try
        {
            var cantidadLocales = await _api.GetCantidadLocales(token);
            localesLabel.Text = $"{cantidadLocales}/3 locales"; // Asegúrate de que localesLabel esté vinculado al Label correcto

            //Habilitar el boton de crear local mientras la cantidad sea menor a 2.
            crearLocalButton.IsEnabled = cantidadLocales < 3;
        }
        catch (Exception ex)
        {
            // Maneja la excepción si algo sale mal
            Console.WriteLine(ex.Message);
        }
    }

    private async void CargarLocales()
    {
        try
        {
            var locales = await _api.ObtenerResumenLocales(token);
            Locales.Clear();
            foreach (var local in locales)
            {

                var partes = local.Direccion.Split(';');
                var latitud = double.Parse(partes[0], CultureInfo.InvariantCulture);
                var longitud = double.Parse(partes[1], CultureInfo.InvariantCulture);

                var direccionLegible = await GeocodificarDireccion(latitud, longitud);
                local.Direccion = direccionLegible;

                Locales.Add(local);
            }
            listaLocales.ItemsSource = Locales; // Asigna los locales a la ListView
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void OnClickShowDetails(object sender, SelectedItemChangedEventArgs e)
    {
        var selectedLocal = (LocalLoad)e.SelectedItem;
        SharedData.SelectedLocalId = selectedLocal.Id; // Asumiendo que LocalLoad tiene una propiedad Id

        Application.Current.MainPage = new LocalShell();
    }

    private async void OnClickCrearLocal(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CrearLocalPage());
    }

    private async Task<string> GeocodificarDireccion(double latitud, double longitud)
    {
        string apiKey = "ANpkZKdf3Kz4yWswvrCz"; // Reemplaza con tu API key real de MapTiler
        string url = $"https://api.maptiler.com/geocoding/{longitud},{latitud}.json?key={apiKey}";
        using var client = new HttpClient();
        var respuesta = await client.GetStringAsync(url);
        dynamic data = JsonConvert.DeserializeObject(respuesta);

        if (data.features != null && data.features.Count > 0)
        {
            // Aquí puedes ajustar para extraer la parte específica de la dirección que prefieras
            return data.features[2].place_name;
        }
        else
        {
            return "Dirección no encontrada";
        }
    }
}