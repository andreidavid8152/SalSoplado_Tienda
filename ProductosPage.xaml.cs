using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
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

    private bool estaPulsado { get; set; }

    public ProductosPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LocalId = SharedData.SelectedLocalId;

        categoriasPicker.SelectedIndex = 0;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        categoriasPicker.SelectedIndex = -1;
    }

    private async void OnCategoriaSeleccionadaChanged(object sender, EventArgs e)
    {

        // Verifica si hay alg�n elemento seleccionado
        if (categoriasPicker.SelectedIndex != -1)
        {
            loadingFrame.IsVisible = true;

            var categoriaSeleccionada = categoriasPicker.SelectedItem.ToString();
            if (categoriaSeleccionada == "Todos")
            {
                CargarProductos();
            }
            else
            {
                try
                {
                    // Llama al m�todo ObtenerProductosPorCategoria del servicio API
                    var productosPorCategoria = await _api.ObtenerProductosPorCategoriaYLocal(LocalId, categoriaSeleccionada, token);

                    // Actualiza el origen de datos del CollectionView con los productos filtrados por categor�a
                    productosCollectionView.ItemsSource = productosPorCategoria;

                    // Verifica si la lista est� vac�a
                    if (productosPorCategoria == null || !productosPorCategoria.Any())
                    {
                        // Muestra un mensaje indicando que no hay productos en esta categor�a
                        var successToast = Toast.Make($"No se encontraron productos para la categor�a '{categoriaSeleccionada}'.", ToastDuration.Short);
                        await successToast.Show();
                    }
                }
                catch (Exception ex)
                {
                    // Si algo sale mal, muestra un mensaje al usuario
                    var successToast = Toast.Make($"Error al cargar los productos: {ex.Message}", ToastDuration.Short);
                    await successToast.Show();
                }
                finally
                {
                    loadingFrame.IsVisible = false;
                }
            }

        }

    }


    private async void OnEditTapped(object sender, TappedEventArgs e)
    {
        if (estaPulsado) return; // Ignora si ya se est� procesando otra acci�n

        estaPulsado = true;
        var producto = e.Parameter as ProductoLocalDetalle; // Asume que tu modelo se llama Producto
        if (producto != null)
        {
            // Aqu� puedes pasar el ID o el objeto completo a la p�gina de edici�n, dependiendo de tu implementaci�n
            await Navigation.PushAsync(new EditarProductoPage(producto.ID));
        }
        estaPulsado = false;
    }


    private async void OnDeleteTapped(object sender, TappedEventArgs e)
    {
        if (estaPulsado) return;

        estaPulsado = true;

        var productoEncontrar = e.Parameter as ProductoLocalDetalle;

        var producto = await _api.ObtenerDetalleProducto(productoEncontrar.ID, token);

        if (producto != null)
        {
            // Mostrar un di�logo de confirmaci�n antes de eliminar
            bool confirmDelete = await DisplayAlert("Confirmaci�n", $"�Est�s seguro de que deseas eliminar el producto '{producto.Nombre}' y todas sus im�genes asociadas?", "Eliminar", "Cancelar");
            if (confirmDelete)
            {
                loadingFrame.IsVisible = true;
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
                        var successToast = Toast.Make("Producto e im�genes asociadas eliminados correctamente.", ToastDuration.Short);
                        await successToast.Show();

                        // Refrescar la lista de productos
                        CargarProductos();
                    }
                }
                catch (Exception ex)
                {
                    // En caso de error, mostrar un mensaje
                    await DisplayAlert("Error", $"Ha ocurrido un error al eliminar el producto o sus im�genes: {ex.Message}", "OK");
                }
                finally
                {
                    loadingFrame.IsVisible = false;
                }
            }
        }
        estaPulsado = false;
    }


    private async void CargarProductos()
    {
        try
        {
            var productos = await _api.ObtenerProductosPorLocal(LocalId, token); // Asume que este m�todo existe en APIService y devuelve una lista de ProductoLocalDetalle
            productosCollectionView.ItemsSource = productos;

            // Verifica si la lista est� vac�a
            if (productos == null || !productos.Any())
            {
                // Muestra un mensaje indicando que no hay productos en esta categor�a
                var successToast = Toast.Make("No se encontraron productos", ToastDuration.Short);
                await successToast.Show();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            loadingFrame.IsVisible = false;
        }
    }

}