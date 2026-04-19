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

    private async void OnFilterTapped(object sender, EventArgs e)
    {
        var sortResult = await DisplayActionSheet("Sort by", "Cancel", null,
            _vm.SortOptions?.ToArray() ?? ["Newest", "Oldest", "Most Liked"]);

        if (sortResult != null && sortResult != "Cancel")
            _vm.SelectedSort = sortResult;

        var difficultyResult = await DisplayActionSheet("Filter by Difficulty", "Cancel", "Reset",
            _vm.DifficultyOptions?.ToArray() ?? ["Easy", "Medium", "Hard"]);

        if (difficultyResult == "Reset")
            _vm.SelectedDifficulty = null;
        else if (difficultyResult != null && difficultyResult != "Cancel")
            _vm.SelectedDifficulty = difficultyResult;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Recipes.Count == 0)
            await _vm.LoadRecipesCommand.ExecuteAsync(null);
    }
}
