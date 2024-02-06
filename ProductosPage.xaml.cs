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
            // Aquí puedes pasar el ID o el objeto completo a la página de edición, dependiendo de tu implementación
            await Navigation.PushAsync(new EditarProductoPage(producto.ID));
        }
    }


    private async void OnDeleteTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Éxito", "Eliminar", "OK");
    }

    private async void CargarProductos()
    {
        try
        {
            var productos = await _api.ObtenerProductosPorLocal(LocalId, token); // Asume que este método existe en APIService y devuelve una lista de ProductoLocalDetalle
            productosCollectionView.ItemsSource = productos;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}