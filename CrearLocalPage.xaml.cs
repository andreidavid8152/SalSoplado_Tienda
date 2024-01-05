using SalSoplado_Tienda.Models;
using SalSoplado_Usuario;
using SalSoplado_Usuario.Services;
using System.Diagnostics;

namespace SalSoplado_Tienda;

public partial class CrearLocalPage : ContentPage
{
    private readonly APIService _api;
    private string token = Preferences.Get("UserToken", string.Empty);
    public CrearLocalPage()
    {
        InitializeComponent();
        _api = App.ServiceProvider.GetService<APIService>();
    }

    private async void OnCrearLocalClicked(object sender, EventArgs e)
    {
        // Crear un DTO a partir de los datos del formulario
        var localCreationDTO = new LocalCreation
        {
            Nombre = entryNombre.Text,
            Descripcion = entryDescripcion.Text,
            Direccion = entryDireccion.Text,
            // Aquí debes añadir las imágenes y cualquier otro campo necesario
        };

        // Validar los datos aquí (opcional)

        try
        {
            // Llamar al servicio de API para crear el local
            var success = await _api.CrearLocal(localCreationDTO, token);
            if (success)
            {
                // Manejar el éxito
                await DisplayAlert("Éxito", "Local creado con éxito", "OK");
                // Opcional: Navegar a otra página o actualizar la interfaz de usuario
            }
        }
        catch (Exception ex)
        {
            // Manejar los errores
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnUploadImageButtonClicked(object sender, EventArgs e)
    {
        if (await CheckAndRequestStoragePermission())
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Por favor selecciona una imagen",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    selectedImage.Source = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"No se pudo seleccionar la imagen: {ex.Message}");
            }
        }
        else
        {
            // Aquí puedes manejar lo que sucede si no se otorgan los permisos
            Debug.WriteLine("Permiso de acceso al almacenamiento denegado.");
        }
    }

    private async Task<bool> CheckAndRequestStoragePermission()
    {
        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (status == PermissionStatus.Granted)
        {
            // El permiso de almacenamiento ha sido concedido
            return true;
        }
        else
        {
            // El permiso de almacenamiento ha sido denegado o no se ha podido obtener
            return false;
        }
    }
}