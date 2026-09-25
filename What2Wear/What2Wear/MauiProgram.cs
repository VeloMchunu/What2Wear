using Microsoft.Extensions.Logging;
using What2Wear.Services;
using What2Wear.Shared.Services;
using CommunityToolkit.Maui;

namespace What2Wear
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // Add Community Toolkit with Fluent UI support
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                });

            // Add device-specific services used by the What2Wear.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            // Add HTTP client for API communication
            builder.Services.AddSingleton<HttpClient>(serviceProvider =>
            {
                var handler = new HttpClientHandler();
#if DEBUG
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://localhost:7118/") // Change to your API URL
                };
                return client;
            });

            // Add services
            builder.Services.AddSingleton<Services.ISecureStorage, Services.SecureStorageImpl>();
            builder.Services.AddSingleton<IApiClient, ApiClient>();
            builder.Services.AddSingleton<IOutfitSuggestionService, OutfitSuggestionService>();

            // Add Pages
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
    }
}
