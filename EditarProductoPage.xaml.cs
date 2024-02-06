using SalSoplado_Tienda.Models;
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

        ProductoId = idProducto;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalId = SharedData.SelectedLocalId;
    }

    private async void OnClickActualizarProducto(object sender, EventArgs e)
    {



    }
}