using ForkFeedMobile.Services;
using ForkFeedMobile.ViewModels;

namespace ForkFeedMobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    private async void OnThemeToggleClicked(object sender, EventArgs e)
    {
        var themeService = IPlatformApplication.Current!.Services.GetRequiredService<ThemeService>();

        var result = await DisplayActionSheet("Choose Theme", "Cancel", null, "☀️ Light", "🌙 Dark", "⚙️ System");

        switch (result)
        {
            case "☀️ Light":
                themeService.SetTheme(AppTheme.Light);
                break;
            case "🌙 Dark":
                themeService.SetTheme(AppTheme.Dark);
                break;
            case "⚙️ System":
                themeService.SetTheme(AppTheme.Unspecified);
                break;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Load data when page appears (handles returning from detail too)
        if (_vm.Recipes.Count == 0)
            await _vm.LoadRecipesCommand.ExecuteAsync(null);
    }
}
