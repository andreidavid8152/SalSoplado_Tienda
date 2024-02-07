using Firebase.Storage;
using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;

namespace SalSoplado_Tienda;

public partial class ProductosPage : ContentPage
{

    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);
    private int LocalId { get; set; }

    public ProductosPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalId = SharedData.SelectedLocalId;
        CargarProductos();
    }

    private async void OnEditTapped(object sender, TappedEventArgs e)
    {
        var producto = e.Parameter as ProductoLocalDetalle; // Asume que tu modelo se llama Producto
        if (producto != null)
        {
            // Aqu� puedes pasar el ID o el objeto completo a la p�gina de edici�n, dependiendo de tu implementaci�n
            await Navigation.PushAsync(new EditarProductoPage(producto.ID));
        }
    }


    private async void OnDeleteTapped(object sender, TappedEventArgs e)
    {
        var productoEncontrar = e.Parameter as ProductoLocalDetalle;

        var producto = await _api.ObtenerDetalleProducto(productoEncontrar.ID, token);

        if (producto != null)
        {
            // Mostrar un di�logo de confirmaci�n antes de eliminar
            bool confirmDelete = await DisplayAlert("Confirmaci�n", $"�Est�s seguro de que deseas eliminar el producto '{producto.Nombre}' y todas sus im�genes asociadas?", "Eliminar", "Cancelar");
            if (confirmDelete)
            {
                try
                {
                    // Eliminar cada imagen asociada al producto
                    foreach (var imageUrl in producto.ImagenesUrls) // Asumiendo que producto.ImagenesUrls es una lista de URLs de im�genes
                    {
                        // Decodificar la URL para obtener el path del archivo
                        var decodedUrl = Uri.UnescapeDataString(imageUrl);
                        var startIndex = decodedUrl.IndexOf("imagenesProductos/") + "imagenesProductos/".Length;
                        var endIndex = decodedUrl.IndexOf("?", startIndex);
                        var fileName = decodedUrl.Substring(startIndex, endIndex - startIndex);

                        // Elimina la imagen de Firebase Storage
                        await new FirebaseStorage("salsoplado.appspot.com")
                              .Child("imagenesProductos")
                              .Child(fileName)
                              .DeleteAsync();
                    }

                    // Eliminar el producto de la base de datos
                    var success = await _api.EliminarProducto(producto.ID, token);
                    if (success)
                    {
                        // Mostrar un mensaje de �xito
                        await DisplayAlert("�xito", "Producto e im�genes asociadas eliminados correctamente.", "OK");

                        // Refrescar la lista de productos
                        CargarProductos();
                    }
                }
                catch (Exception ex)
                {
                    // En caso de error, mostrar un mensaje
                    await DisplayAlert("Error", $"Ha ocurrido un error al eliminar el producto o sus im�genes: {ex.Message}", "OK");
                }
            }
        }
    }


    private async void CargarProductos()
    {
        try
        {
            var productos = await _api.ObtenerProductosPorLocal(LocalId, token); // Asume que este m�todo existe en APIService y devuelve una lista de ProductoLocalDetalle
            productosCollectionView.ItemsSource = productos;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}