namespace SalSoplado_Tienda;

public partial class ProductosPage : ContentPage
{
    public ProductosPage()
    {
        InitializeComponent();
    }

    private async void OnFrameTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Éxito", "Inicio de sesión exitoso", "OK");
    }


}