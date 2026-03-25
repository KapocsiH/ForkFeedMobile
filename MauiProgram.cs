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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services (singleton so mock data persists across pages)
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

            // Pages
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<RecipeDetailPage>();
            builder.Services.AddTransient<CookingModePage>();
            builder.Services.AddTransient<AddRecipePage>();
            builder.Services.AddTransient<FavoritesPage>();
            builder.Services.AddTransient<ProfilePage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
