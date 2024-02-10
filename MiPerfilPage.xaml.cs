using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
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
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarPerfil(); // Esto asegura que cada vez que la página aparezca, tu perfil se cargue/recargue.
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

        // Activar el ActivityIndicator
        loadingFrame.IsVisible = true;

        // Deshabilitar botones
        ButtonGuardar.IsEnabled = false;
        ButtonCerrarSesion.IsEnabled = false;

        try
        {
            var usuario = new UserEdit
            {
                Nombre = NombreEntry.Text,
                Email = EmailEntry.Text,
                Username = UsernameEntry.Text,
                Password = string.IsNullOrEmpty(PasswordEntry.Text) ? "****" : PasswordEntry.Text
            };

            if (IsValid(usuario, out List<string> errorMessages))
            {
                string token = Preferences.Get("UserToken", string.Empty);
                var result = await _api.EditarPerfil(usuario, token);

                if (result)
                {
                    var successToast = Toast.Make("Perfil actualizado correctamente", ToastDuration.Short);
                    await successToast.Show();

                    ResetUI();
                }
                else
                {
                    var successToast = Toast.Make("No se pudo actualizar el perfil.", ToastDuration.Short);
                    await successToast.Show();
                }
            }
            else
            {
                foreach (var errorMessage in errorMessages)
                {
                    var successToast = Toast.Make(errorMessage, ToastDuration.Short);
                    await successToast.Show();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Ocurrió un error al actualizar el perfil: " + ex.Message, "OK");
        }
        finally
        {
            // Volver a habilitar los botones, independientemente del resultado
            loadingFrame.IsVisible = false;
            ButtonGuardar.IsEnabled = true;
            ButtonCerrarSesion.IsEnabled = true;
        }
    }

    private void ResetUI()
    {
        // Limpia los campos de texto y restablece cualquier estado de UI deseado a sus valores predeterminados
        PasswordEntry.Text = string.Empty;

        // Asegúrate de desenfocar cualquier Entry activo para eliminar el cursor
        NombreEntry.Unfocus();
        EmailEntry.Unfocus();
        UsernameEntry.Unfocus();
        PasswordEntry.Unfocus();

        // Restablecer cualquier otro estado de la UI que necesites
        CargarPerfil();
    }

    private async void OnClickCerrarSesion(object sender, EventArgs e)
    {
        // Deshabilitar botones para evitar múltiples clics
        ButtonGuardar.IsEnabled = false;
        ButtonCerrarSesion.IsEnabled = false;

        try
        {
            // Aquí va tu lógica para cerrar sesión
            // Por ejemplo, eliminar el token guardado y cualquier otra información relevante
            Preferences.Remove("UserToken");
            Preferences.Remove("SavedAddress");
            Preferences.Remove("SavedLatitude");
            Preferences.Remove("SavedLongitude");

            // Navegar al usuario a la pantalla de inicio de sesión o a la pantalla principal
            Application.Current.MainPage = new NavigationPage(new LoginPage())
            {
                BarBackgroundColor = Color.FromHex("#d9e3f1"),
                BarTextColor = Color.FromHex("#000000")
            };
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Ocurrió un error al cerrar sesión: " + ex.Message, "OK");

            // En caso de error, vuelve a habilitar los botones para permitir que el usuario intente nuevamente
            ButtonGuardar.IsEnabled = true;
            ButtonCerrarSesion.IsEnabled = true;
        }
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