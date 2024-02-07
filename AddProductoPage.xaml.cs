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

    // Lista para almacenar las im�genes seleccionadas
    private List<ImageSource> selectedImages = new List<ImageSource>();
    private const int MaxImages = 3; // N�mero m�ximo de im�genes permitidas

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

        // Restablece el Picker a su valor por defecto (por ejemplo, el primer �tem o ninguno)
        categoriaPicker.SelectedIndex = -1; // Esto hace que no se seleccione ning�n �tem

        // Restablece el DatePicker a la fecha actual
        fechaVencimientoPicker.Date = DateTime.Now;

        // Si tienes m�s controles que necesitan ser limpiados, a��delos aqu�

        // Tambi�n limpiamos las im�genes seleccionadas y la UI relacionada
        selectedImages.Clear(); // Limpia la lista de im�genes seleccionadas
        imagesContainer.Children.Clear(); // Remueve todas las im�genes del contenedor en la UI
    }

    private async void OnClickCrearProducto(object sender, EventArgs e)
    {
        // Inicializa el modelo con valores que sabes que no son nulos,
        // como cadenas vac�as para las propiedades de tipo string.
        ProductoCreationDTO productoInput = new ProductoCreationDTO
        {
            LocalID = SharedData.SelectedLocalId,
            Nombre = nombreEntry.Text ?? string.Empty, // Evita nulos en las cadenas
            Categoria = categoriaPicker.SelectedItem?.ToString() ?? string.Empty, // Maneja el caso nulo
        };

        // Usa TryParse para conversiones seguras de los campos num�ricos
        if (int.TryParse(cantidadEntry.Text, out int cantidad))
        {
            productoInput.Cantidad = cantidad;
        }
        else
        {
            await DisplayAlert("Error", "La cantidad ingresada no es v�lida.", "OK");
            return; // Sale del m�todo si la conversi�n falla
        }

        if (decimal.TryParse(precioOriginalEntry.Text, out decimal precioOriginal))
        {
            productoInput.PrecioOriginal = precioOriginal;
        }
        else
        {
            await DisplayAlert("Error", "El precio original ingresado no es v�lido.", "OK");
            return; // Sale del m�todo si la conversi�n falla
        }

        if (decimal.TryParse(precioOfertaEntry.Text, out decimal precioDescuento))
        {
            productoInput.PrecioOferta = precioDescuento;
        }
        else
        {
            await DisplayAlert("Error", "El precio de descuento ingresado no es v�lido.", "OK");
            return; // Sale del m�todo si la conversi�n falla
        }

        // Validaci�n de precios
        if (productoInput.PrecioOferta >= productoInput.PrecioOriginal)
        {
            await DisplayAlert("Error", "El precio con descuento debe ser menor al precio original.", "OK");
            return; // Sale del m�todo si la conversi�n falla
        }


        // Verificar si se han seleccionado 3 imagenes
        if (selectedImages.Count != MaxImages)
        {
            await DisplayAlert("Advertencia", "Debes subir 3 im�genes", "OK");
            return;
        }

        // Asumimos que fechaVencimientoPicker siempre tiene una fecha v�lida,
        // ya que es un control DatePicker.
        productoInput.FechaVencimiento = fechaVencimientoPicker.Date;

        // Subir im�genes y esperar a que todas se hayan subido
        var imagenesSubidas = await subirImagenesSeleccionadas();
        productoInput.ImagenesUrls = imagenesSubidas;

        // Aqu� podr�as llamar a IsValid() para validar productoInput si mantienes la l�gica de validaci�n
        if (IsValid(productoInput, out List<string> errorMessages))
        {

            try
            {
                // L�gica para enviar productoInput a tu API
                var success = await _api.CrearProducto(productoInput, token);

                if (success)
                {
                    // Manejar el �xito
                    await DisplayAlert("�xito", "Producto creado con �xito", "OK");

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
            await DisplayAlert("Advertencia", "Ya has seleccionado el m�ximo de im�genes permitidas", "OK");
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
        // Necesitas identificar qu� imagen fue tocada.
        var imageTapped = sender as Image;
        int imageIndex = imagesContainer.Children.IndexOf(imageTapped);

        // Si no hay imagen o el �ndice es incorrecto, no hacer nada.
        if (imageTapped == null || imageIndex == -1) return;

        // Si ya hay una imagen seleccionada en este �ndice, preguntar si desea cambiarla.
        if (selectedImages.Count > imageIndex)
        {
            bool answer = await DisplayAlert("Cambiar imagen", "�Quieres cambiar esta imagen?", "S�", "No");
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