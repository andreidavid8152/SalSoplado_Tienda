using SalSoplado_Usuario;

namespace SalSoplado_Tienda;

public partial class AjustesPage : ContentPage
{
    public AjustesPage()
    {
        InitializeComponent();
    }

    private async void OnClickVolver(object sender, EventArgs e)
    {
        Application.Current.MainPage = new AppShell();
    }
}