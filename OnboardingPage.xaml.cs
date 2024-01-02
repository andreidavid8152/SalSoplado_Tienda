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
        // Establecer la página principal utilizando el servicio
        Application.Current.MainPage = new NavigationPage(new LoginPage())
        {
            BarBackgroundColor = Color.FromHex("#d9e3f1")
        };
    }
}