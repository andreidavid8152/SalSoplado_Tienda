using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;
using System.Diagnostics;
using Firebase.Storage;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

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

        Preferences.Set("SavedLatitude", String.Empty);
        Preferences.Set("SavedLongitude", String.Empty);
    }

    private async void OnCambiarUbicacionClicked(object sender, EventArgs e)
    {
        var direccionModalPage = new ubicacionPage();

        // Cambia aquí para manejar una cadena en lugar de latitud y longitud
        direccionModalPage.OnLocationSelected = (address) =>
        {
            LocationLabel.Text = address; // Actualizar el Label con la dirección
        };

        await Navigation.PushModalAsync(direccionModalPage);
    }

    private async void OnCrearLocalClicked(object sender, EventArgs e)
    {

        var latitud = Preferences.Get("SavedLatitude", "");
        var longitud = Preferences.Get("SavedLongitude", "");

        //Verificar si la ubicacion ha sido seleccionada
        if (latitud.Equals("") && longitud.Equals(""))
        {
            await DisplayAlert("Advertencia", "Selecciona una ubicacion.", "OK");
            return;
        }

        // Verificar si se han seleccionado 3 imágenes
        if (selectedImages.Count != MaxImages)
        {
            await DisplayAlert("Advertencia", "Debes subir 3 imágenes", "OK");
            return;
        }

        // Subir imágenes y esperar a que todas se hayan subido
        var imagenesSubidas = await subirImagenesSeleccionadas();

        // Crear un DTO a partir de los datos del formulario
        var localCreationDTO = new LocalCreation
        {
            Nombre = entryNombre.Text,
            Descripcion = entryDescripcion.Text,
            Direccion = latitud + ";" + longitud,
            ImagenesUrls = imagenesSubidas
        };

        try
        {
            // Llamar al servicio de API para crear el local
            var success = await _api.CrearLocal(localCreationDTO, token);
            if (success)
            {
                // Manejar el éxito
                await DisplayAlert("Éxito", "Local creado con éxito", "OK");
            }
        }
        catch (Exception ex)
        {
            // Manejar los errores
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task<List<String>> subirImagenesSeleccionadas()
    {
        List<String> imagenes = new List<String>();

        foreach (var imageSource in selectedImages)
        {
            var imageUrl = await SubirImagenAFirebase(imageSource);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                imagenes.Add(imageUrl);
            }
        }

        return imagenes;
    }

    private async Task<string> SubirImagenAFirebase(ImageSource imageSource)
    {
        var stream = await ConvertirImageSourceAStream(imageSource);
        if (stream == null)
        {
            return null;
        }

        var fileName = Guid.NewGuid().ToString() + ".jpg";

        string storageImage;
        try
        {
            storageImage = await new FirebaseStorage("salsoplado.appspot.com")
                                     .Child("imagenesLocales")
                                     .Child(fileName)
                                     .PutAsync(stream);
        }
        finally
        {
            stream.Dispose();
        }

        return storageImage;
    }


    private async Task<Stream> ConvertirImageSourceAStream(ImageSource imageSource)
    {
        if (imageSource == null)
        {
            return null;
        }

        if (imageSource is FileImageSource fileImageSource)
        {
            string filePath = fileImageSource.File;
            if (File.Exists(filePath))
            {
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
        }
        else if (imageSource is UriImageSource uriImageSource)
        {
            var uri = uriImageSource.Uri;
            if (uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStreamAsync();
                    }
                }
            }
        }
        else if (imageSource is StreamImageSource streamImageSource)
        {
            var cancellationToken = System.Threading.CancellationToken.None;
            var streamFunc = streamImageSource.Stream;
            var stream = await streamFunc(cancellationToken);
            if (stream != null && stream.CanRead)
            {
                return stream;
            }
        }

        return null;
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
                ImageSource imageSource = ImageSource.FromStream(() =>
                {
                    var stream = result.OpenReadAsync().Result;
                    return stream;
                });

                selectedImages.Add(imageSource);

                var newImage = new Image
                {
                    Source = imageSource,
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 100,
                    WidthRequest = 100,
                    Margin = 5,
                };

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += OnImageTapped;
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