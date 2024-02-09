using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;
using System.Diagnostics;
using Firebase.Storage;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace SalSoplado_Tienda;

public partial class CrearLocalPage : ContentPage
{
    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);

    // Lista para almacenar las imágenes seleccionadas
    private ImageSource selectedImage; // Solo una imagen

    public CrearLocalPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();

        Preferences.Set("SavedLatitude", String.Empty);
        Preferences.Set("SavedLongitude", String.Empty);
        UpdateImageContainer();
    }

    private void UpdateImageContainer()
    {
        imagesContainer.Children.Clear(); // Limpia el contenedor de imágenes

        if (selectedImage != null)
        {
            var image = new Image
            {
                Source = selectedImage,
                Aspect = Aspect.AspectFill,
                HeightRequest = 100,
                WidthRequest = 100,
                Margin = 5,
            };
            imagesContainer.Children.Add(image);
        }
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

        ButtonCrear.IsEnabled = false;
        ButtonUbicacion.IsEnabled = false;
        ButtonLogo.IsEnabled = false;

        loadingFrame.IsVisible = true;

        try
        {
            // Validación del nombre
            if (entryNombre.Text == null || entryNombre.Text.Length < 5)
            {
                var successToast = Toast.Make("El nombre debe tener al menos 5 caracteres.", ToastDuration.Short);
                await successToast.Show();
                return; // Detiene la ejecución si no pasa la validación
            }

            // Validación de horas
            if (Inicio.Time >= Fin.Time)
            {
                var successToast = Toast.Make("La hora de inicio debe ser menor que la hora de cierre.", ToastDuration.Short);
                await successToast.Show();
                return; // Detiene la ejecución si no pasa la validación
            }

            // Validación del teléfono
            if (string.IsNullOrWhiteSpace(entryTelefono.Text) || !entryTelefono.Text.StartsWith("09") || entryTelefono.Text.Length != 10)
            {
                var successToast = Toast.Make("El numero de telefono debe ser valido.", ToastDuration.Short);
                await successToast.Show();
                return; // Detiene la ejecución si no pasa la validación
            }

            var latitud = Preferences.Get("SavedLatitude", "");
            var longitud = Preferences.Get("SavedLongitude", "");

            //Verificar si la ubicacion ha sido seleccionada
            if (latitud.Equals("") && longitud.Equals(""))
            {
                var successToast = Toast.Make("Selecciona una ubicacion.", ToastDuration.Short);
                await successToast.Show();
                return;
            }

            // Verificar si se ha seleccionado la imagen
            if (selectedImage == null)
            {
                var successToast = Toast.Make("Debes subir el logo", ToastDuration.Short);
                await successToast.Show();
                return;
            }

            // Subir la imagen seleccionada y obtener la URL
            var imageUrl = await SubirImagenSeleccionada();

            // Verificar si la imagen se subió correctamente
            if (string.IsNullOrEmpty(imageUrl))
            {
                var successToast = Toast.Make("Hubo un problema al subir el logo.", ToastDuration.Short);
                await successToast.Show();
                return;
            }

            // Crear un DTO a partir de los datos del formulario
            var localCreationDTO = new LocalCreation
            {
                Nombre = entryNombre.Text,
                HoraInicio = Inicio.Time,
                HoraFin = Fin.Time,
                Telefono = entryTelefono.Text,
                Direccion = latitud + ";" + longitud,
                Logo = imageUrl
            };

            // Llamar al servicio de API para crear el local
            var success = await _api.CrearLocal(localCreationDTO, token);
            if (success)
            {
                // Manejar el éxito
                var successToast = Toast.Make("Local creado con éxito", ToastDuration.Short);
                await successToast.Show();
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            // Manejar los errores
            var successToast = Toast.Make($"{ex.Message}", ToastDuration.Short);
            await successToast.Show();
        }
        finally
        {
            // Volver a habilitar los botones, independientemente del resultado
            loadingFrame.IsVisible = false;

            ButtonCrear.IsEnabled = true;
            ButtonUbicacion.IsEnabled = true;
            ButtonLogo.IsEnabled = true;
        }
    }

    private async Task<string> SubirImagenSeleccionada()
    {
        if (selectedImage == null)
        {
            return null;
        }

        var stream = await ConvertirImageSourceAStream(selectedImage);
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
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Por favor selecciona una imagen",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                selectedImage = ImageSource.FromStream(() =>
                {
                    var stream = result.OpenReadAsync().Result;
                    return stream;
                });

                UpdateImageContainer(); // Actualiza la UI con la nueva imagen
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"No se pudo seleccionar la imagen: {ex.Message}");
        }
    }
}