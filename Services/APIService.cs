using Microsoft.Maui.Media;
using Newtonsoft.Json;
using SalSoplado_Tienda.Models;
using SalSoplado_Usuario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Usuario.Services
{
    public class APIService
    {

        public static string _baseUrl;
        public HttpClient _httpClient;

        // Constructor: inicializa el URL base y el cliente HTTP.
        public APIService()
        {
            _baseUrl = "https://apisalsoplado20240208202806.azurewebsites.net/api/";
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        //AUTH

        public async Task<bool> Registro(UserRegistration usuario)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}Auth/register", usuario);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<String> Login(Login usuario)
        {

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}Auth/loginPropietario", usuario);
            if (response.IsSuccessStatusCode)
            {
                // Deserializar el cuerpo de la respuesta para obtener el token
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

                return tokenResponse.Token;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }

        }



        //USUARIO

        public async Task<UserRegistration> GetPerfil(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{_baseUrl}Usuarios/getPerfil");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserRegistration>(content);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<bool> EditarPerfil(UserEdit usuario, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}Usuarios/editarPerfil", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }



        //PROPIETARIO

        public async Task<int> GetCantidadLocales(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{_baseUrl}Propietarios/cantidadLocales");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<int>(content);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        //LOCALES
        public async Task<bool> CrearLocal(LocalCreation local, string token)
        {
            // Añade el token de autorización en la cabecera de la petición
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Realiza la petición POST al endpoint CrearLocal
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}Locales/CrearLocal", local);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<List<LocalLoad>> ObtenerResumenLocales(string token)
        {
            // Añade el token como header de autorización
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_baseUrl}Locales/ObtenerResumenLocalesPropietario");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var locales = JsonConvert.DeserializeObject<List<LocalLoad>>(content);
                return locales;
            }

            throw new Exception("No se pudo obtener los locales desde la API.");
        }

        public async Task<LocalDetalle> ObtenerDetalleLocal(int idLocal, string token)
        {
            // Añade el token como header de autorización
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Realiza la solicitud GET al endpoint ObtenerDetalleLocal
            var response = await _httpClient.GetAsync($"{_baseUrl}Locales/ObtenerDetalleLocal/{idLocal}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var detalleLocal = JsonConvert.DeserializeObject<LocalDetalle>(content);
                return detalleLocal;
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception(errorMessage);
        }

        public async Task<bool> CrearProducto(ProductoCreationDTO producto, string token)
        {
            // Añade el token de autorización en la cabecera de la petición
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Realiza la petición POST al endpoint de creación de productos
            var response = await _httpClient.PostAsJsonAsync("Productos/CrearProducto", producto);

            if (response.IsSuccessStatusCode)
            {
                // Producto creado exitosamente
                return true;
            }
            else
            {
                // Algo salió mal, lee el mensaje de error de la respuesta
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<List<ProductoLocalDetalle>> ObtenerProductosPorLocal(int localId, string token)
        {
            // Añade el token como header de autorización
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Realiza la solicitud GET al endpoint ObtenerProductosPorLocal
            var response = await _httpClient.GetAsync($"Productos/ObtenerProductosPorLocal/{localId}");

            if (response.IsSuccessStatusCode)
            {
                // Si la petición es exitosa, deserializa el contenido de la respuesta a una lista de ProductoLocalDetalle
                var content = await response.Content.ReadAsStringAsync();
                var productos = JsonConvert.DeserializeObject<List<ProductoLocalDetalle>>(content);
                return productos;
            }
            else
            {
                // Si algo sale mal, lee el mensaje de error de la respuesta y lanza una excepción
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<ProductoDetalleEdit> ObtenerDetalleProducto(int productoId, string token)
        {
            // Añade el token como header de autorización
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Realiza la solicitud GET al endpoint ObtenerProductoPorId
            var response = await _httpClient.GetAsync($"Productos/ObtenerProductoPorId/{productoId}");

            if (response.IsSuccessStatusCode)
            {
                // Si la petición es exitosa, deserializa el contenido de la respuesta a ProductoDetalleDTO
                var content = await response.Content.ReadAsStringAsync();
                var productoDetalle = JsonConvert.DeserializeObject<ProductoDetalleEdit>(content);
                return productoDetalle;
            }
            else
            {
                // Si algo sale mal, lee el mensaje de error de la respuesta y lanza una excepción
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<bool> EditarProducto(ProductoDetalleEdit productoEdit, string token)
        {
            // Añade el token de autorización en la cabecera de la petición
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Serializa el objeto productoEditDTO a JSON
            var jsonContent = new StringContent(JsonConvert.SerializeObject(productoEdit), Encoding.UTF8, "application/json");

            // Realiza la petición PUT al endpoint EditarProducto
            var response = await _httpClient.PutAsync($"Productos/EditarProducto/{productoEdit.ID}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // La edición del producto fue exitosa
                return true;
            }
            else
            {
                // Algo salió mal, lee el mensaje de error de la respuesta
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<bool> EliminarProducto(int productoId, string token)
        {
            // Añade el token de autorización en la cabecera de la petición
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Realiza la petición DELETE al endpoint EliminarProducto
            var response = await _httpClient.DeleteAsync($"Productos/EliminarProducto/{productoId}");

            if (response.IsSuccessStatusCode)
            {
                // Producto eliminado exitosamente
                return true;
            }
            else
            {
                // Algo salió mal, lee el mensaje de error de la respuesta
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        public async Task<List<ProductoLocalDetalle>> ObtenerProductosPorCategoriaYLocal(int localId, string categoria, string token)
        {
            // Asegúrate de que el token de autorización se incluye en la cabecera de la solicitud
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Realiza la solicitud GET al endpoint, incluyendo el localId y la categoría en la URL
            var response = await _httpClient.GetAsync($"Productos/ObtenerProductosPorCategoriaYLocal/{localId}/{categoria}");

            if (response.IsSuccessStatusCode)
            {
                // Si la solicitud es exitosa, lee el contenido de la respuesta
                var content = await response.Content.ReadAsStringAsync();
                // Deserializa el contenido de la respuesta en una lista de objetos ProductoLocalDetalle
                var productos = JsonConvert.DeserializeObject<List<ProductoLocalDetalle>>(content);
                return productos;
            }
            else
            {
                // Si hay un problema con la solicitud, lee el mensaje de error de la respuesta
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }


    }
}
