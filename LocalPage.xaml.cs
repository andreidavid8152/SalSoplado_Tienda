using Newtonsoft.Json;
using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;
using System.Globalization;

namespace SalSoplado_Tienda;

public partial class LocalPage : ContentPage
{
    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);
    private int LocalId { get; set; }

    public LocalPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalId = SharedData.SelectedLocalId;
        CargarLocal();
    }

    private async void CargarLocal()
    {
        try
        {
            var local = await _api.ObtenerDetalleLocal(LocalId, token);
            localNombre.Text = local.Nombre;

            var partes = local.Direccion.Split(';');
            var latitud = double.Parse(partes[0], CultureInfo.InvariantCulture);
            var longitud = double.Parse(partes[1], CultureInfo.InvariantCulture);

            var direccionLegible = await GeocodificarDireccion(latitud, longitud);
            localUbicacion.Text = direccionLegible; // Asumiendo que quieres reemplazar el texto existente

            localImagen.Source = local.Imagen;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
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