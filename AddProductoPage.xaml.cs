using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Firebase.Storage;
using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SalSoplado_Tienda;

public partial class AddProductoPage : ContentPage
{

    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);

    // Lista para almacenar las imágenes seleccionadas
    private List<ImageSource> selectedImages = new List<ImageSource>();
    private const int MaxImages = 3; // Número máximo de imágenes permitidas

    public AddProductoPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Limpia los Entry
        nombreEntry.Text = string.Empty;
        cantidadEntry.Text = string.Empty;
        precioOriginalEntry.Text = string.Empty;
        precioOfertaEntry.Text = string.Empty;

        // Restablece el Picker a su valor por defecto (por ejemplo, el primer ítem o ninguno)
        categoriaPicker.SelectedIndex = -1; // Esto hace que no se seleccione ningún ítem

        // Restablece el DatePicker a la fecha actual
        fechaVencimientoPicker.Date = DateTime.Now;

        // Si tienes más controles que necesitan ser limpiados, añádelos aquí

        // También limpiamos las imágenes seleccionadas y la UI relacionada
        selectedImages.Clear(); // Limpia la lista de imágenes seleccionadas
        imagesContainer.Children.Clear(); // Remueve todas las imágenes del contenedor en la UI
    }

    private async void OnClickCrearProducto(object sender, EventArgs e)
    {
        ButtonCrear.IsEnabled = false;
        ButtonImagenes.IsEnabled = false;

        loadingFrame.IsVisible = true;
        try
        {
            // Inicializa el modelo con valores que sabes que no son nulos,
            // como cadenas vacías para las propiedades de tipo string.
            ProductoCreationDTO productoInput = new ProductoCreationDTO
            {
                LocalID = SharedData.SelectedLocalId,
                Nombre = nombreEntry.Text ?? string.Empty, // Evita nulos en las cadenas
                Categoria = categoriaPicker.SelectedItem?.ToString() ?? string.Empty, // Maneja el caso nulo
            };

            if (string.IsNullOrWhiteSpace(productoInput.Nombre) || productoInput.Nombre.Length > 255)
            {
                var nombreInvalidoToast = Toast.Make("El nombre del producto no puede estar vacío.", ToastDuration.Short);
                await nombreInvalidoToast.Show();
                return; // Salimos del método si el nombre no es válido
            }


            // Usa TryParse para conversiones seguras de los campos numéricos
            if (int.TryParse(cantidadEntry.Text, out int cantidad))
            {
                productoInput.Cantidad = cantidad;
            }
            else
            {
                var successToast = Toast.Make("La cantidad ingresada no es válida", ToastDuration.Short);
                await successToast.Show();
                return; // Sale del método si la conversión falla
            }

            if (decimal.TryParse(precioOriginalEntry.Text, out decimal precioOriginal))
            {
                productoInput.PrecioOriginal = precioOriginal;
            }
            else
            {
                var successToast = Toast.Make("El precio original ingresado no es válido.", ToastDuration.Short);
                await successToast.Show();
                return; // Sale del método si la conversión falla
            }

            if (decimal.TryParse(precioOfertaEntry.Text, out decimal precioDescuento))
            {
                productoInput.PrecioOferta = precioDescuento;
            }
            else
            {
                var successToast = Toast.Make("El precio de descuento ingresado no es válido.", ToastDuration.Short);
                await successToast.Show();
                return; // Sale del método si la conversión falla
            }

            // Validación de precios
            if (productoInput.PrecioOferta >= productoInput.PrecioOriginal)
            {
                var successToast = Toast.Make("El precio con descuento debe ser menor al precio original.", ToastDuration.Short);
                await successToast.Show();
                return; // Sale del método si la conversión falla
            }


            // Verificar si se han seleccionado 3 imagenes
            if (selectedImages.Count != MaxImages)
            {
                var successToast = Toast.Make("Debes subir 3 imágenes", ToastDuration.Short);
                await successToast.Show();
                return;
            }

            // Asumimos que fechaVencimientoPicker siempre tiene una fecha válida,
            // ya que es un control DatePicker.
            productoInput.FechaVencimiento = fechaVencimientoPicker.Date;

            // Subir imágenes y esperar a que todas se hayan subido
            var imagenesSubidas = await subirImagenesSeleccionadas();
            productoInput.ImagenesUrls = imagenesSubidas;

            // Aquí podrías llamar a IsValid() para validar productoInput si mantienes la lógica de validación
            if (IsValid(productoInput, out List<string> errorMessages))
            {

                try
                {
                    // Lógica para enviar productoInput a tu API
                    var success = await _api.CrearProducto(productoInput, token);

                    if (success)
                    {
                        // Manejar el éxito
                        var successToast = Toast.Make("Producto creado con éxito", ToastDuration.Short);
                        await successToast.Show();

                        // Navegar a LocalPage
                        await Shell.Current.GoToAsync("//LocalPage");
                    }
                }
                catch (Exception ex)
                {
                    // Manejar los errores
                    await DisplayAlert("Error", ex.Message, "OK");
                }

            }
            else
            {
                foreach (var errorMessage in errorMessages)
                {
                    await DisplayAlert("Error", errorMessage, "OK");
                }
            }


        }
        catch (Exception ex)
        {
            // Manejar los errores
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            loadingFrame.IsVisible = false;

            ButtonCrear.IsEnabled = true;
            ButtonImagenes.IsEnabled = true;
        }

    }


    private bool IsValid(ProductoCreationDTO productoInput, out List<string> errorMessages)
    {
        var context = new ValidationContext(productoInput, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(productoInput, context, validationResults, true);

        errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();
        return isValid;
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
                                     .Child("imagenesProductos")
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
            var successToast = Toast.Make("Ya has seleccionado el máximo de imágenes permitidas", ToastDuration.Short);
            await successToast.Show();
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
        var imageTapped = sender as Image;
        if (imageTapped == null) return;

        int imageIndex = imagesContainer.Children.IndexOf(imageTapped);
        if (imageIndex == -1) return;

        bool answer = await DisplayAlert("Cambiar imagen", "¿Quieres cambiar esta imagen?", "Sí", "No");
        if (!answer) return;

        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona una imagen",
            FileTypes = FilePickerFileType.Images
        });

        if (result != null)
        {
            var newImageSource = ImageSource.FromFile(result.FullPath);

            // Asegúrate de actualizar la lista de imágenes seleccionadas
            if (selectedImages.Count > imageIndex)
            {
                selectedImages[imageIndex] = newImageSource;
            }
            else
            {
                selectedImages.Add(newImageSource);
            }

            // Actualiza la UI
            imageTapped.Source = newImageSource;
        }
    }


}