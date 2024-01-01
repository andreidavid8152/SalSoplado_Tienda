using SalSoplado_Usuario.Models;
using System.Collections.ObjectModel;

namespace SalSoplado_Usuario;

public partial class OnboardingViewModel
{
    public ObservableCollection<OnboardingItem> Items { get; set; }

    public OnboardingViewModel()
    {
        Items = new ObservableCollection<OnboardingItem>
        {
            new OnboardingItem { Title = "Crea tu cuenta", Description = "Reg�strate para empezar a utilizar todas las funciones de la aplicaci�n. �Es r�pido y f�cil!", ImageUrl = "registro_onboarding.png" },
            new OnboardingItem { Title = "Inicia sesi�n", Description = "Accede a tu cuenta para continuar donde lo dejaste.", ImageUrl = "login_onboarding.png" },
            new OnboardingItem { Title = "Agrega tu local", Description = "Configura tu local en la app para que los clientes puedan encontrarte.", ImageUrl = "local_onboarding.png" },
            new OnboardingItem { Title = "A�ade tus productos", Description = "Sube los detalles de tus productos para que los usuarios puedan ver lo que ofreces.", ImageUrl = "anadirproducto_onboarding.png" },
            new OnboardingItem { Title = "�Listo para vender!", Description = "Ya est�s todo listo para empezar a vender y crecer tu negocio.", ImageUrl = "vender_onboarding.png" }
        };
        Items.Last().IsLast = true;
    }
}