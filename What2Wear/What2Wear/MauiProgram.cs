using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using What2Wear.Services;
using What2Wear.Shared.Services;

namespace What2Wear
{
    public static class MauiProgram
    {
        // Must match the port in What2Wear.Web launchSettings.json (https profile)
        private const int ApiPort = 7118;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                });

            // Device-specific services used by What2Wear.Shared
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            // Platform-specific API settings
            builder.Services.AddSingleton(_ => new ApiSettings(ResolveBaseUrl()));

            // HTTP client
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var settings = sp.GetRequiredService<ApiSettings>();

                var handler = new HttpClientHandler();
#if DEBUG
                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(settings.BaseUrl),
                    Timeout = TimeSpan.FromSeconds(15)
                };
            });

            // Services
            builder.Services.AddSingleton<Services.ISecureStorage, Services.SecureStorageImpl>();
            builder.Services.AddSingleton<IApiClient, ApiClient>();
            builder.Services.AddSingleton<IOutfitSuggestionService, OutfitSuggestionService>();

            // Pages
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<SignupPage>();
            builder.Services.AddSingleton<PhotosPage>();
            builder.Services.AddSingleton<CapturePhotoPage>();
            builder.Services.AddSingleton<OutfitThemesPage>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static string ResolveBaseUrl()
        {
#if DEBUG
            // Android emulator reaches the host PC via 10.0.2.2
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return DeviceInfo.DeviceType == DeviceType.Virtual
                    ? $"https://10.0.2.2:{ApiPort}/"
                    : $"https://192.168.0.100:{ApiPort}/"; // physical device: your PC's LAN IP
            }

            // iOS simulator, MacCatalyst and Windows share the host's localhost
            // (physical iOS device also needs the LAN IP)
            return $"https://localhost:{ApiPort}/";
#else
            return "https://your-production-api.example.com/";
#endif
        }
    }

    public record ApiSettings(string BaseUrl);
}