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
        var userInput = (UserRegistration)this.BindingContext;

        if (IsValid(userInput, out List<string> errorMessages))
        {
            try
            {
                await _api.Registro(userInput);
                await DisplayAlert("Éxito", "Registro completado con éxito", "OK");

                // Redirigir al usuario a la página de inicio de sesión
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
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

    private bool IsValid(UserRegistration userInput, out List<string> errorMessages)
    {
        var context = new ValidationContext(userInput, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(userInput, context, validationResults, true);

        errorMessages = validationResults.Select(r => r.ErrorMessage).ToList();
        return isValid;
    }
}