using SalSoplado_Tienda.Models;

namespace SalSoplado_Tienda;

public partial class LocalPage : ContentPage
{
    private int LocalId { get; set; }

    public LocalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalId = SharedData.SelectedLocalId;
    }

}