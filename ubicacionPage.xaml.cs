namespace SalSoplado_Tienda;

public partial class ubicacionPage : ContentPage
{
    public Action<string> OnLocationSelected { get; set; }
    public ubicacionPage()
    {
        InitializeComponent();
        webViewMapa.Navigated += OnWebViewNavigated;
    }

    private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        var url = e.Url;
        if (url.Contains("#saveLocation"))
        {
            var dataPart = url.Split(new[] { "#saveLocation:" }, StringSplitOptions.None).LastOrDefault();
            if (!string.IsNullOrEmpty(dataPart))
            {
                var parts = dataPart.Split(',');
                if (parts.Length >= 3)
                {
                    var latitude = parts[0];
                    var longitude = parts[1];
                    var address = System.Net.WebUtility.UrlDecode(parts[2]);

                    // Guardar las coordenadas y la dirección en Preferences
                    Preferences.Set("SavedLatitude", latitude);
                    Preferences.Set("SavedLongitude", longitude);

                    // Notificar a la página principal
                    OnLocationSelected?.Invoke(address);

                    // Cierra el modal
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopModalAsync();
                    });
                }
            }
        }
        else
        {
            LoadCurrentLocation();
        }
    }



    async void LoadCurrentLocation()
    {
        try
        {
            var savedLatitude = Preferences.Get("SavedLatitude", string.Empty);
            var savedLongitude = Preferences.Get("SavedLongitude", string.Empty);
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (!string.IsNullOrEmpty(savedLatitude) && !string.IsNullOrEmpty(savedLongitude))
            {
                // Si hay coordenadas guardadas, usar esas para inicializar el mapa
                await webViewMapa.EvaluateJavaScriptAsync($"initMap({savedLatitude}, {savedLongitude})");
            }
            else
            {
                if (status == PermissionStatus.Granted)
                {
                    var location = await Geolocation.GetLastKnownLocationAsync();
                    if (location != null)
                    {
                        await webViewMapa.EvaluateJavaScriptAsync($"initMap({location.Latitude}, {location.Longitude})");
                    }
                    else
                    {
                        location = await Geolocation.GetLocationAsync(new GeolocationRequest
                        {
                            DesiredAccuracy = GeolocationAccuracy.Medium,
                            Timeout = TimeSpan.FromSeconds(30)
                        });
                    }
                }
            }

        }
        catch (Exception ex)
        {
            // Manejar excepciones (como usuario negando el permiso)
            Console.WriteLine(ex.Message);
        }
    }
}