using Microsoft.Maui.Media;
using SalSoplado_Tienda;
using SalSoplado_Tienda.Models;
using SalSoplado_Usuario.Services;
using System.Collections.ObjectModel;

namespace SalSoplado_Usuario;

public partial class LocalesPage : ContentPage
{
    private readonly APIService _api;
    private ObservableCollection<LocalLoad> Locales { get; set; }
    private string token = Preferences.Get("UserToken", string.Empty);
    public LocalesPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
        Locales = new ObservableCollection<LocalLoad>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarCantidadLocales(); // Llama a este m�todo para actualizar la informaci�n cada vez que la p�gina aparezca
        CargarLocales();
    }

    private async void CargarCantidadLocales()
    {
        try
        {
            var cantidadLocales = await _api.GetCantidadLocales(token);
            localesLabel.Text = $"{cantidadLocales}/3 locales"; // Aseg�rate de que localesLabel est� vinculado al Label correcto

            //Habilitar el boton de crear local mientras la cantidad sea menor a 2.
            crearLocalButton.IsEnabled = cantidadLocales < 3;
        }
        catch (Exception ex)
        {
            // Maneja la excepci�n si algo sale mal
            Console.WriteLine(ex.Message);
        }
    }

    private async void CargarLocales()
    {
        try
        {
            var locales = await _api.ObtenerResumenLocales(token);
            Locales.Clear();
            foreach (var local in locales)
            {
                Locales.Add(local);
            }
            listaLocales.ItemsSource = Locales; // Asigna los locales a la ListView
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void OnClickShowDetails(object sender, SelectedItemChangedEventArgs e)
    {
        var selectedLocal = (LocalLoad)e.SelectedItem;
        SharedData.SelectedLocalId = selectedLocal.Id; // Asumiendo que LocalLoad tiene una propiedad Id

        Application.Current.MainPage = new LocalShell();
    }

    private async void OnClickCrearLocal(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CrearLocalPage());
    }
}