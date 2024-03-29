using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SalSoplado_Usuario.Models;
using SalSoplado_Usuario.Services;
using System.ComponentModel.DataAnnotations;

namespace SalSoplado_Usuario;

public partial class LoginPage : ContentPage
{
    private readonly APIService _api;
    public LoginPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>(); // Obtener la instancia de APIService del contenedor
        this.BindingContext = new Login();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        string token = Preferences.Get("UserToken", string.Empty);
        if (!token.Equals(string.Empty))
        {
            // Establecer la AppShell como la nueva p�gina principal
            Application.Current.MainPage = new AppShell();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        UsernameEntry.Text = null;
        PasswordEntry.Text = null;
    }

    private async void OnClickLogin(object sender, EventArgs e)
    {

        ButtonLogin.IsEnabled = false;

        var userLogin = (Login)this.BindingContext;
        if (IsValid(userLogin, out List<string> errorMessages))
        {
            try
            {
                var token = await _api.Login(userLogin);
                Preferences.Set("UserToken", token); // Guardar el token

                // Usar Toast para el mensaje de �xito
                var successToast = Toast.Make("Inicio de sesi�n exitoso", ToastDuration.Short);
                await successToast.Show();

                // Animar la opacidad de la p�gina de login a 0
                await this.FadeTo(0, 400); // 400 es la duraci�n en milisegundos

                // Establecer la AppShell como la nueva p�gina principal
                Application.Current.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                // Usar Toast para el mensaje de error
                var errorToast = Toast.Make($"Error: {ex.Message}", ToastDuration.Short);
                await errorToast.Show();
            }
            finally
            {
                ButtonLogin.IsEnabled = true;
            }
        }
        else
        {
            foreach (var errorMessage in errorMessages)
            {
                // Usar Toast para cada mensaje de error
                var errorToast = Toast.Make(errorMessage, ToastDuration.Short);
                await errorToast.Show();
            }

            ButtonLogin.IsEnabled = true;
        }
    }

    private bool IsValid(Login userLogin, out List<string> errorMessages)
    {
        var context = new ValidationContext(userLogin, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(userLogin, context, validationResults, true);

        errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();
        return isValid;
    }

    private void OnShowPasswordCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        // Invierte el estado de IsPassword basado en si el CheckBox est� marcado o no
        PasswordEntry.IsPassword = !e.Value;
    }

    private void OnClickRegistrarse(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RegistroPage());
    }

}