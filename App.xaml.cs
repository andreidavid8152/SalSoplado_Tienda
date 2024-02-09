using SalSoplado_Usuario.Services;

namespace SalSoplado_Usuario
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        public App()
        {
            InitializeComponent();

            // Configurar el contenedor de servicios
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Construir el proveedor de servicios
            _serviceProvider = services.BuildServiceProvider();

            // Revisa si es la primera vez que se inicia la app
            bool isFirstLaunch = Preferences.Get("IsFirstLaunch", true);

            if (isFirstLaunch)
            {
                // Si es la primera vez, muestra la pantalla de onboarding
                MainPage = new OnboardingPage();
                Preferences.Set("IsFirstLaunch", false);
            }
            else
            {
                // Si no es la primera vez, muestra la página de login
                MainPage = new NavigationPage((Page)_serviceProvider.GetService(typeof(LoginPage)))
                {
                    BarBackgroundColor = Color.FromHex("#d9e3f1"),
                    BarTextColor = Color.FromHex("#000000")
                };
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Registrar APIService como un singleton para que la misma instancia se utilice en toda la app
            services.AddSingleton<APIService>();
            services.AddTransient<LoginPage>();
        }

        public static IServiceProvider ServiceProvider => ((App)Current)._serviceProvider;
    }
}
