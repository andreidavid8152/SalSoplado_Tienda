using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SalSoplado_Usuario.Models;
using SalSoplado_Usuario.Services;
using System.ComponentModel.DataAnnotations;

namespace SalSoplado_Usuario;

public partial class RegistroPage : ContentPage
{
    private readonly APIService _api;
    public RegistroPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
        this.BindingContext = new UserRegistration();
    }

    private async void OnClickRegistrarse(object sender, EventArgs e)
    {

        ButtonRegistrarse.IsEnabled = false;

        var userInput = (UserRegistration)this.BindingContext;

        if (IsValid(userInput, out List<string> errorMessages))
        {
            try
            {
                await _api.Registro(userInput);

                var successToast = Toast.Make("Registro completado con éxito", ToastDuration.Short);
                await successToast.Show();


                // Redirigir al usuario a la página de inicio de sesión
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                var successToast = Toast.Make($"Error: {ex.Message}", ToastDuration.Short);
                await successToast.Show();
            }
            finally
            {
                ButtonRegistrarse.IsEnabled = true;
            }
        }
        else
        {
            foreach (var errorMessage in errorMessages)
            {
                var successToast = Toast.Make($"{errorMessage}", ToastDuration.Short);
                await successToast.Show();
            }

            ButtonRegistrarse.IsEnabled = true;
        }
    }

    private void OnShowPasswordCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        // Invierte el estado de IsPassword basado en si el CheckBox está marcado o no
        PasswordEntry.IsPassword = !e.Value;
    }


    private bool IsValid(UserRegistration userInput, out List<string> errorMessages)
    {
        var context = new ValidationContext(userInput, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(userInput, context, validationResults, true);

        errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();
        return isValid;
    }
}