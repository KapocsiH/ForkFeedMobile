using CommunityToolkit.Maui;
using ForkFeedMobile.Services;
using ForkFeedMobile.ViewModels;
using ForkFeedMobile.Views;
using Microsoft.Extensions.Logging;

namespace ForkFeedMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ── API Service ──────────────────────────────────────
            // REAL API – uses the Vercel backend
            builder.Services.AddSingleton<IApiService>(sp =>
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://forkfeed.vercel.app/api/"),
                    Timeout = TimeSpan.FromSeconds(30)
                };
                return new ApiService(httpClient);
            });

            // MOCK API – uncomment the line below (and comment the block above) to use mock data
            // builder.Services.AddSingleton<IApiService, MockApiService>();

            // Services (singleton so state persists across pages)
            builder.Services.AddSingleton<RecipeService>();
            builder.Services.AddSingleton<FavoritesService>();
            builder.Services.AddSingleton<AuthService>();

            // ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<RecipeDetailViewModel>();
            builder.Services.AddTransient<CookingModeViewModel>();
            builder.Services.AddTransient<AddRecipeViewModel>();
            builder.Services.AddTransient<FavoritesViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<ForgotPasswordViewModel>();

            // Pages
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<RecipeDetailPage>();
            builder.Services.AddTransient<CookingModePage>();
            builder.Services.AddTransient<AddRecipePage>();
            builder.Services.AddTransient<FavoritesPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ForgotPasswordPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
