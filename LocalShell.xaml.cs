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
        // Comprueba si la navegación es hacia una pestaña diferente
        if (e.Source == ShellNavigationSource.ShellSectionChanged)
        {
            // Obtén la ruta de la página raíz de la pestaña actual
            var currentTab = this.CurrentItem.CurrentItem;
            var route = currentTab.Route;

            // Navega a la página raíz de la pestaña actual
            await this.GoToAsync($"//{route}");
        }
    }
}