namespace SalSoplado_Usuario;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage()
    {
        InitializeComponent();
        BindingContext = new OnboardingViewModel();
    }

    private void cerrarOnBoarding(object sender, EventArgs e)
    {
        // Establecer la p�gina principal utilizando el servicio
        Application.Current.MainPage = new LoginPage();
    }
}