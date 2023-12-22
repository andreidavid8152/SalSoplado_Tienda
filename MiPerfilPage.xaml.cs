using SalSoplado_Usuario.Models;
using SalSoplado_Usuario.Services;
using System.ComponentModel.DataAnnotations;

namespace SalSoplado_Usuario;

public partial class MiPerfilPage : ContentPage
{
    private readonly APIService _api;
    public MiPerfilPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
        CargarPerfil();
    }

    private async void CargarPerfil()
    {
        try
        {
            string token = Preferences.Get("UserToken", string.Empty);
            var perfil = await _api.GetPerfil(token);
            // Aquí asignas los valores a tus EntryCells
            NombreEntry.Text = perfil.Nombre;
            EmailEntry.Text = perfil.Email;
            UsernameEntry.Text = perfil.Username;
            Username.Text = perfil.Username;
        }
        catch (Exception ex)
        {
            // Manejar el error
            Console.WriteLine(ex.Message);
        }
    }

    private async void OnClickGuardar(object sender, EventArgs e)
    {
        try
        {
            var usuario = new UserEdit
            {
                Nombre = NombreEntry.Text,
                Email = EmailEntry.Text,
                Username = UsernameEntry.Text,
                Password = PasswordEntry.Text
            };

            if (IsValid(usuario, out List<string> errorMessages))
            {
                string token = Preferences.Get("UserToken", string.Empty);
                var result = await _api.EditarPerfil(usuario, token);

                if (result)
                {
                    await DisplayAlert("Éxito", "Perfil actualizado correctamente.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo actualizar el perfil.", "OK");
                }
            }
            else
            {
                foreach (var errorMessage in errorMessages)
                {
                    await DisplayAlert("Error", errorMessage, "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Ocurrió un error al actualizar el perfil: " + ex.Message, "OK");
        }
    }

    private async void OnClickCerrarSesion(object sender, EventArgs e)
    {
        // Eliminar el token guardado
        Preferences.Remove("UserToken");
        // Navegar al usuario a la pantalla de inicio de sesión o a la pantalla principal
        Application.Current.MainPage = new NavigationPage(new LoginPage())
        {
            BarBackgroundColor = Color.FromHex("#d9e3f1")
        };
    }


    private bool IsValid(UserEdit userInput, out List<string> errorMessages)
    {
        var context = new ValidationContext(userInput, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(userInput, context, validationResults, true);

        errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();
        return isValid;
    }
}