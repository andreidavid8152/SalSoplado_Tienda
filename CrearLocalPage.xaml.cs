using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;
using System.Diagnostics;

namespace SalSoplado_Tienda;

public partial class CrearLocalPage : ContentPage
{
    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);

    // Lista para almacenar las imágenes seleccionadas
    private List<ImageSource> selectedImages = new List<ImageSource>();
    private const int MaxImages = 3; // Número máximo de imágenes permitidas

    public CrearLocalPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
        webViewMapa.Navigated += OnWebViewMapNavigated;

        webViewMapa.Navigating += (s, e) =>
        {
            // Desactivar el desplazamiento del ScrollView cuando se empieza a navegar en el WebView
            mainScrollView.IsEnabled = false;
        };

        webViewMapa.Navigated += (s, e) =>
        {
            // Reactivar el desplazamiento del ScrollView una vez completada la navegación en el WebView
            mainScrollView.IsEnabled = true;
        };

    }

    private void OnWebViewMapNavigated(object sender, WebNavigatedEventArgs e)
    {
        var url = e.Url;
        if (url.Contains("#saveLocation"))
        {
            // Extraer la latitud, longitud y dirección de la URL
            // y asignarlos a los campos correspondientes en tu formulario
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

    private async void OnCrearLocalClicked(object sender, EventArgs e)
    {

        // Verificar si se han seleccionado 3 imágenes
        if (selectedImages.Count != MaxImages)
        {
            await DisplayAlert("Advertencia", "Debes subir 3 imágenes", "OK");
            return;
        }

        // Crear un DTO a partir de los datos del formulario
        var localCreationDTO = new LocalCreation
        {
            Nombre = entryNombre.Text,
            Descripcion = entryDescripcion.Text,
            //Direccion = entryDireccion.Text,
            // Aquí debes añadir las imágenes y cualquier otro campo necesario
        };

        // Validar los datos aquí (opcional)

        try
        {
            // Llamar al servicio de API para crear el local
            var success = await _api.CrearLocal(localCreationDTO, token);
            if (success)
            {
                // Manejar el éxito
                await DisplayAlert("Éxito", "Local creado con éxito", "OK");
                // Opcional: Navegar a otra página o actualizar la interfaz de usuario
            }
        }
        catch (Exception ex)
        {
            // Manejar los errores
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnUploadImageButtonClicked(object sender, EventArgs e)
    {
        if (selectedImages.Count >= MaxImages)
        {
            await DisplayAlert("Advertencia", "Ya has seleccionado el máximo de imágenes permitidas", "OK");
            return;
        }

        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Por favor selecciona una imagen",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                var imageSource = ImageSource.FromStream(() => stream);

                selectedImages.Add(imageSource);

                var newImage = new Image
                {
                    Source = imageSource,
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 100,
                    WidthRequest = 100,
                    Margin = 5,
                };

                // Agrega el TapGestureRecognizer a la imagen
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += OnImageTapped; // Aquí asignas el manejador de eventos que ya modificaste.
                newImage.GestureRecognizers.Add(tapGestureRecognizer);

                imagesContainer.Children.Add(newImage);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"No se pudo seleccionar la imagen: {ex.Message}");
        }
    }

    private async void OnImageTapped(object sender, EventArgs e)
    {
        // Necesitas identificar qué imagen fue tocada.
        var imageTapped = sender as Image;
        int imageIndex = imagesContainer.Children.IndexOf(imageTapped);

        // Si no hay imagen o el índice es incorrecto, no hacer nada.
        if (imageTapped == null || imageIndex == -1) return;

        // Si ya hay una imagen seleccionada en este índice, preguntar si desea cambiarla.
        if (selectedImages.Count > imageIndex)
        {
            bool answer = await DisplayAlert("Cambiar imagen", "¿Quieres cambiar esta imagen?", "Sí", "No");
            if (!answer) return; // Si el usuario elige 'No', simplemente regresa.
        }

        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Por favor selecciona una imagen",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                var imageSource = ImageSource.FromStream(() => stream);

                // Reemplaza la imagen actual o agrega una nueva si es que no hay ninguna.
                if (selectedImages.Count > imageIndex)
                {
                    selectedImages[imageIndex] = imageSource; // Reemplaza la imagen existente.
                }
                else
                {
                    selectedImages.Add(imageSource); // Agrega la nueva imagen.
                }

                // Actualiza la imagen en la interfaz de usuario.
                (sender as Image).Source = imageSource;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"No se pudo seleccionar la imagen: {ex.Message}");
        }
    }


}