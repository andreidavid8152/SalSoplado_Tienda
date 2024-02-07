using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;

namespace SalSoplado_Tienda;

public partial class EditarProductoPage : ContentPage
{
    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);
    private int LocalId { get; set; }
    private int ProductoId { get; set; }

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
        }
        catch (Exception ex)
        {
            // Manejar el error
            Console.WriteLine(ex.Message);
        }
    }

    private async void OnClickActualizarProducto(object sender, EventArgs e)
    {



    }
}