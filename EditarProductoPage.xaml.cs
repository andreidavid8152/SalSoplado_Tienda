using Firebase.Storage;
using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;
using System.Diagnostics;

namespace SalSoplado_Tienda;

public partial class EditarProductoPage : ContentPage
{
    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);
    private int LocalId { get; set; }
    private int ProductoId { get; set; }

    // Lista para almacenar las URLs de imágenes existentes
    private List<string> existingImageUrls = new List<string>();

    List<ImageSource> temporaryNewImages = new List<ImageSource>();

    HashSet<int> imagesToReplaceIndexes = new HashSet<int>();


    public EditarProductoPage(int idProducto)
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
        ProductoId = idProducto;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalId = SharedData.SelectedLocalId;
        CargarProducto();
    }

    private async void CargarProducto()
    {
        try
        {
            var producto = await _api.ObtenerDetalleProducto(ProductoId, token);
            // Aquí asignas los valores a tus EntryCells
            nombreEntry.Text = producto.Nombre;
            cantidadEntry.Text = producto.Cantidad.ToString();
            fechaVencimientoPicker.Date = producto.FechaVencimiento;
            precioOriginalEntry.Text = producto.PrecioOriginal.ToString();
            precioOfertaEntry.Text = producto.PrecioOferta.ToString();

            int categoriaIndex = categoriaPicker.ItemsSource.IndexOf(producto.Categoria);
            categoriaPicker.SelectedIndex = categoriaIndex >= 0 ? categoriaIndex : 0;

            // Cargar y mostrar imágenes existentes
            foreach (var imageUrl in producto.ImagenesUrls)
            {

                existingImageUrls.Add(imageUrl);

                var image = new Image
                {
                    Source = imageUrl,
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 100,
                    WidthRequest = 100,
                    Margin = 5
                };

                // Añadir un gesto de toque para cada imagen para permitir la edición
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += OnImageTapped;
                image.GestureRecognizers.Add(tapGestureRecognizer);

                imagesContainer.Children.Add(image);
            }
        }
        catch (Exception ex)
        {
            // Manejar el error
            Console.WriteLine(ex.Message);
        }
    }

    private async void OnClickActualizarProducto(object sender, EventArgs e)
    {

        // Subir imágenes nuevas y obtener sus URLs
        var nuevasUrls = await subirImagenesSeleccionadas();

        // Preparar la lista final de URLs para actualizar en la base de datos
        List<string> urlsFinales = new List<string>(existingImageUrls); // Comenzar con las URLs existentes

        int nuevaUrlIndex = 0; // Índice para las nuevas URLs

        foreach (int index in imagesToReplaceIndexes) // Para cada imagen reemplazada
        {
            if (nuevaUrlIndex < nuevasUrls.Count) // Verificar que haya nuevas URLs disponibles
            {
                urlsFinales[index] = nuevasUrls[nuevaUrlIndex++]; // Reemplazar la URL en la posición correcta
            }
        }

        ProductoDetalleEdit producto = new ProductoDetalleEdit
        {
            ID = ProductoId,
            LocalID = SharedData.SelectedLocalId,
            Nombre = nombreEntry.Text ?? string.Empty, // Evita nulos en las cadenas
            Categoria = categoriaPicker.SelectedItem?.ToString() ?? string.Empty, // Maneja el caso nulo
        };

        // Usa TryParse para conversiones seguras de los campos numéricos
        if (int.TryParse(cantidadEntry.Text, out int cantidad))
        {
            producto.Cantidad = cantidad;
        }
        else
        {
            await DisplayAlert("Error", "La cantidad ingresada no es válida.", "OK");
            return; // Sale del método si la conversión falla
        }

        if (decimal.TryParse(precioOriginalEntry.Text, out decimal precioOriginal))
        {
            producto.PrecioOriginal = precioOriginal;
        }
        else
        {
            await DisplayAlert("Error", "El precio original ingresado no es válido.", "OK");
            return; // Sale del método si la conversión falla
        }

        if (decimal.TryParse(precioOfertaEntry.Text, out decimal precioDescuento))
        {
            producto.PrecioOferta = precioDescuento;
        }
        else
        {
            await DisplayAlert("Error", "El precio de descuento ingresado no es válido.", "OK");
            return; // Sale del método si la conversión falla
        }

        // Validación de precios
        if (producto.PrecioOferta >= producto.PrecioOriginal)
        {
            await DisplayAlert("Error", "El precio con descuento debe ser menor al precio original.", "OK");
            return; // Sale del método si la conversión falla
        }

        producto.FechaVencimiento = fechaVencimientoPicker.Date;

        producto.ImagenesUrls = urlsFinales;

        try
        {
            // Lógica para enviar productoInput a tu API
            var success = await _api.EditarProducto(producto, token);

            if (success)
            {
                // Manejar el éxito
                await DisplayAlert("Éxito", "Producto editado con éxito", "OK");

                // Navegar a LocalPage
                await Navigation.PopAsync();
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

        // Subir solo las imágenes nuevas o las que reemplazan a las existentes
        foreach (var imageSource in temporaryNewImages)
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
                                     .Child("imagenesProductosEditadas")
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


    private async void OnImageTapped(object sender, EventArgs e)
    {
        var imageTapped = sender as Image;
        int imageIndex = imagesContainer.Children.IndexOf(imageTapped);
        bool isExistingImage = imageIndex < existingImageUrls.Count;

        bool answer = await DisplayAlert("Cambiar imagen", "¿Quieres cambiar esta imagen?", "Sí", "No");
        if (!answer) return;

        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Por favor selecciona una imagen",
            FileTypes = FilePickerFileType.Images,
        });

        if (result != null)
        {
            var newImageSource = ImageSource.FromStream(() => result.OpenReadAsync().Result);

            if (isExistingImage)
            {
                if (imagesToReplaceIndexes.Contains(imageIndex))
                {
                    // Encuentra la posición correspondiente en temporaryNewImages y actualízala
                    int tempIndex = imagesToReplaceIndexes.ToList().IndexOf(imageIndex);
                    if (tempIndex >= 0 && tempIndex < temporaryNewImages.Count)
                    {
                        temporaryNewImages[tempIndex] = newImageSource;
                    }
                }
                else
                {
                    // Marca la imagen existente para reemplazo
                    imagesToReplaceIndexes.Add(imageIndex);
                    // Almacena la nueva imagen temporalmente
                    temporaryNewImages.Add(newImageSource);
                }
            }
            else
            {
                // Para imágenes nuevas, simplemente añádelas a la lista
                temporaryNewImages.Add(newImageSource);
            }

            // Actualiza la imagen en la UI
            imageTapped.Source = newImageSource;
        }
    }


}