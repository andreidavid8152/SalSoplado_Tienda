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
            _baseUrl = "http://10.0.2.2:5260/api/";
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


    }
}
