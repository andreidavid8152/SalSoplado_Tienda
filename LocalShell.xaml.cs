namespace SalSoplado_Tienda;

public partial class LocalShell : Shell
{
    public LocalShell()
    {
        InitializeComponent();
        this.Navigated += LocalShell_Navigated;
    }

    private async void LocalShell_Navigated(object sender, ShellNavigatedEventArgs e)
    {
        // Comprueba si la navegaci�n es hacia una pesta�a diferente
        if (e.Source == ShellNavigationSource.ShellSectionChanged)
        {
            // Obt�n la ruta de la p�gina ra�z de la pesta�a actual
            var currentTab = this.CurrentItem.CurrentItem;
            var route = currentTab.Route;

            // Navega a la p�gina ra�z de la pesta�a actual
            await this.GoToAsync($"//{route}");
        }
    }
}