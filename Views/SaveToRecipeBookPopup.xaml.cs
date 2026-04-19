using CommunityToolkit.Maui.Views;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.Views;

public partial class SaveToRecipeBookPopup : Popup
{
    private readonly IApiService _apiService;
    private readonly int _recipeId;
    private readonly int _userId;
    private ApiRecipeBook? _selectedBook;

    public SaveToRecipeBookPopup(IApiService apiService, int recipeId, int userId)
    {
        InitializeComponent();
        _apiService = apiService;
        _recipeId = recipeId;
        _userId = userId;
        TitleLabel.Text = "Ment\u00E9s receptf\u00FCzetbe";
        EmptyLabel.Text = "Nincs receptf\u00FCzeted.";
        SaveButton.Text = "Ment\u00E9s";
        CancelButton.Text = "M\u00E9gsem";
        _ = LoadBooksAsync();
    }

    private async Task LoadBooksAsync()
    {
        try
        {
            var result = await _apiService.GetUserRecipeBooksAsync(_userId, 1, 100);
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            if (result.IsSuccess && result.Data?.RecipeBooks.Count > 0)
            {
                BooksCollection.ItemsSource = result.Data.RecipeBooks;
                BooksCollection.IsVisible = true;
            }
            else
            {
                EmptyLabel.IsVisible = true;
            }
        }
        catch
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            ShowError("Nem sikerült betölteni a receptfüzeteket.");
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedBook = e.CurrentSelection.FirstOrDefault() as ApiRecipeBook;
        SaveButton.IsEnabled = _selectedBook != null;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_selectedBook == null) return;

        SaveButton.IsEnabled = false;
        ErrorLabel.IsVisible = false;

        var result = await _apiService.AddRecipeToBookAsync(_selectedBook.Id, _recipeId);

        if (result.IsSuccess)
        {
            await CloseAsync(true);
        }
        else
        {
            ShowError(result.ErrorMessage ?? "Nem sikerült menteni.");
            SaveButton.IsEnabled = true;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await CloseAsync(false);
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
