using System.Text;
using System.Text.Json;
using What2Wear.Shared.Models;
using Microsoft.Maui;
using Microsoft.Extensions.Logging;

namespace What2Wear.Services
{
    public interface IApiClient
    {
        Task<AuthResponse> SignupAsync(SignupRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<PhotoUploadResponse> UploadPhotoAsync(Stream fileStream, string fileName, PhotoUploadRequest request);
        Task<(bool Success, IEnumerable<PhotoDto> Photos)> GetPhotosAsync();
        Task<bool> DeletePhotoAsync(int photoId);
        Task<OutfitSuggestionResponse> GetOutfitSuggestionAsync(OutfitSuggestionRequest request);
        string? GetToken();
        void SetToken(string? token);
        bool IsAuthenticated();
    }

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ISecureStorage _secureStorage;
        private readonly ILogger<ApiClient> _logger;
        private const string TokenKey = "auth_token";

        public ApiClient(HttpClient httpClient, ISecureStorage secureStorage, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _secureStorage = secureStorage;
            _logger = logger;
        }

        public async Task<AuthResponse> SignupAsync(SignupRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/signup", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResponse>(responseContent) ?? new AuthResponse { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signup error");
                return new AuthResponse { Success = false, Message = "Signup failed: " + ex.Message };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResponse>(responseContent) ?? new AuthResponse { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return new AuthResponse { Success = false, Message = "Login failed: " + ex.Message };
            }
        }

        public async Task<PhotoUploadResponse> UploadPhotoAsync(Stream fileStream, string fileName, PhotoUploadRequest request)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "file", fileName);
                content.Add(new StringContent(request.ClothingType), "clothingType");
                if (!string.IsNullOrEmpty(request.Tags))
                    content.Add(new StringContent(request.Tags), "tags");
                if (!string.IsNullOrEmpty(request.Notes))
                    content.Add(new StringContent(request.Notes), "notes");

                var response = await _httpClient.PostAsync("api/photos/upload", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PhotoUploadResponse>(responseContent) ?? new PhotoUploadResponse { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Photo upload error");
                return new PhotoUploadResponse { Success = false, Message = "Upload failed: " + ex.Message };
            }
        }

        public async Task<(bool Success, IEnumerable<PhotoDto> Photos)> GetPhotosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/photos");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(responseContent);
                    var root = jsonDoc.RootElement;
                    if (root.TryGetProperty("photos", out var photosElement))
                    {
                        var photos = JsonSerializer.Deserialize<IEnumerable<PhotoDto>>(photosElement.GetRawText()) ?? new List<PhotoDto>();
                        return (true, photos);
                    }
                }
                return (false, new List<PhotoDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get photos error");
                return (false, new List<PhotoDto>());
            }
        }

        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/photos/{photoId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete photo error");
                return false;
            }
        }

        public async Task<OutfitSuggestionResponse> GetOutfitSuggestionAsync(OutfitSuggestionRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/outfits/suggest", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OutfitSuggestionResponse>(responseContent) ?? new OutfitSuggestionResponse 
                { 
                    Success = false, 
                    Message = "Invalid response" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outfit suggestion error");
                return new OutfitSuggestionResponse 
                { 
                    Success = false, 
                    Message = "Error: " + ex.Message 
                };
            }
        }

        public string? GetToken()
        {
            return MainThread.IsMainThread
                ? SecureStorage.GetAsync(TokenKey).Result
                : SecureStorage.GetAsync(TokenKey).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void SetToken(string? token)
        {
            if (token == null)
            {
                SecureStorage.Remove(TokenKey);
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                SecureStorage.SetAsync(TokenKey, token).Wait();
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetToken());
        }
    }

    public interface ISecureStorage
    {
        Task SetAsync(string key, string value);
        Task<string?> GetAsync(string key);
        void Remove(string key);
    }

    public class SecureStorageImpl : ISecureStorage
    {
        public async Task SetAsync(string key, string value)
        {
            await SecureStorage.SetAsync(key, value);
        }

        public async Task<string?> GetAsync(string key)
        {
            try
            {
                return await SecureStorage.GetAsync(key);
            }
            catch
            {
                return null;
            }
        }

        public void Remove(string key)
        {
            SecureStorage.Remove(key);
        }
    }
}
