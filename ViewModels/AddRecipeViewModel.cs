using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class AddRecipeViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly AuthService _authService;
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _recipeTitle = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _selectedDifficulty = "Easy";

    [ObservableProperty]
    private string _cookingTimeMinutes = string.Empty;

    [ObservableProperty]
    private string _newIngredientName = string.Empty;

    [ObservableProperty]
    private string _newIngredientQty = string.Empty;

    [ObservableProperty]
    private string _newIngredientUnit = string.Empty;

    [ObservableProperty]
    private string _newStepDescription = string.Empty;

    [ObservableProperty]
    private ImageSource? _selectedImageSource;

    [ObservableProperty]
    private string? _selectedImagePath;

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    private bool _isLoadingData;

    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<RecipeStep> Steps { get; } = new();
    public List<string> DifficultyOptions { get; } = new() { "Easy", "Medium", "Hard" };
    public ObservableCollection<SelectableCategory> AvailableCategories { get; } = new();
    public ObservableCollection<SelectableTag> AvailableTags { get; } = new();

    public AddRecipeViewModel(RecipeService recipeService, AuthService authService, IApiService apiService)
    {
        _recipeService = recipeService;
        _authService = authService;
        _apiService = apiService;
        Title = "Add Recipe";
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (AvailableCategories.Count > 0 && AvailableTags.Count > 0)
            return;

        IsLoadingData = true;
        try
        {
            var categoriesTask = _apiService.GetCategoriesAsync();
            var tagsTask = _apiService.GetTagsAsync();

            await Task.WhenAll(categoriesTask, tagsTask);

            var categoriesResult = await categoriesTask;
            var tagsResult = await tagsTask;

            if (categoriesResult.IsSuccess && categoriesResult.Data?.Categories != null)
            {
                AvailableCategories.Clear();
                foreach (var cat in categoriesResult.Data.Categories)
                    AvailableCategories.Add(new SelectableCategory { Id = cat.Id, Name = cat.Name });
            }
            else
            {
                await Shell.Current.DisplayAlert("Hiba", "Nem sikerült betölteni a kategóriákat.", "OK");
            }

            if (tagsResult.IsSuccess && tagsResult.Data?.Tags != null)
            {
                AvailableTags.Clear();
                foreach (var tag in tagsResult.Data.Tags)
                    AvailableTags.Add(new SelectableTag { Id = tag.Id, Name = tag.Name });
            }
            else
            {
                await Shell.Current.DisplayAlert("Hiba", "Nem sikerült betölteni a címkéket.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hiba", $"Adatok betöltése sikertelen: {ex.Message}", "OK");
        }
        finally
        {
            IsLoadingData = false;
        }
    }

    [RelayCommand]
    private void ToggleTag(SelectableTag tag)
    {
        tag.IsSelected = !tag.IsSelected;
    }

    [RelayCommand]
    private void ToggleCategory(SelectableCategory category)
    {
        category.IsSelected = !category.IsSelected;
    }

    [RelayCommand]
    private void AddIngredient()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientName)) return;

        double? parsedQty = null;
        if (!string.IsNullOrWhiteSpace(NewIngredientQty))
        {
            if (!double.TryParse(NewIngredientQty.Trim(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var qty))
            {
                Shell.Current.DisplayAlert("Validation", "Quantity must be a valid number (e.g. 200, 0.5).", "OK");
                return;
            }
            parsedQty = qty;
        }

        Ingredients.Add(new Ingredient
        {
            Name = NewIngredientName.Trim(),
            Quantity = parsedQty,
            Unit = NewIngredientUnit.Trim()
        });

        NewIngredientName = string.Empty;
        NewIngredientQty = string.Empty;
        NewIngredientUnit = string.Empty;
    }

    [RelayCommand]
    private void RemoveIngredient(Ingredient ingredient)
    {
        Ingredients.Remove(ingredient);
    }

    [RelayCommand]
    private void AddStep()
    {
        if (string.IsNullOrWhiteSpace(NewStepDescription)) return;

        Steps.Add(new RecipeStep
        {
            StepNumber = Steps.Count + 1,
            Description = NewStepDescription.Trim()
        });

        NewStepDescription = string.Empty;
    }

    [RelayCommand]
    private void RemoveStep(RecipeStep step)
    {
        Steps.Remove(step);
        for (int i = 0; i < Steps.Count; i++)
            Steps[i].StepNumber = i + 1;
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a recipe photo"
            });

            if (result != null)
            {
                SelectedImagePath = result.FullPath;
                SelectedImageSource = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied",
                "Camera/gallery permission is required.", "OK");
        }
        catch
        {
            // TODO
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a recipe photo"
            });

            if (result != null)
            {
                SelectedImagePath = result.FullPath;
                SelectedImageSource = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied",
                "Camera permission is required.", "OK");
        }
        catch
        {
            // TODO
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(RecipeTitle))
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter a recipe title.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedImagePath))
        {
            await Shell.Current.DisplayAlert("Validation", "Please select an image for the recipe.", "OK");
            return;
        }

        if (Ingredients.Count == 0)
        {
            await Shell.Current.DisplayAlert("Validation", "Please add at least one ingredient.", "OK");
            return;
        }

        if (Steps.Count == 0)
        {
            await Shell.Current.DisplayAlert("Validation", "Please add at least one step.", "OK");
            return;
        }

        if (!int.TryParse(CookingTimeMinutes, out var cookingTime) || cookingTime <= 0)
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter a valid cooking time in minutes.", "OK");
            return;
        }

        if (!_authService.IsLoggedIn)
        {
            await Shell.Current.DisplayAlert("Error", "You must be logged in to create a recipe.", "OK");
            return;
        }

        IsBusy = true;
        SaveRecipeCommand.NotifyCanExecuteChanged();

        try
        {
            var preparationTime = int.Parse(CookingTimeMinutes);

            var categoryIds = AvailableCategories
                .Where(c => c.IsSelected)
                .Select(c => c.Id)
                .ToList();
            if (categoryIds.Count == 0) categoryIds = null;

            var tagIds = AvailableTags
                .Where(t => t.IsSelected)
                .Select(t => t.Id)
                .ToList();

            var (success, recipeId, error) = await _recipeService.CreateRecipeAsync(
                RecipeTitle.Trim(),
                Description.Trim(),
                SelectedDifficulty,
                preparationTime,
                Ingredients.ToList(),
                Steps.ToList(),
                SelectedImagePath,
                categoryIds,
                tagIds.Count > 0 ? tagIds : null);

            if (!success)
            {
                await Shell.Current.DisplayAlert("Error", error ?? "Failed to create recipe.", "OK");
                return;
            }

            IsSaved = true;
            await Shell.Current.DisplayAlert("Success", "Recipe created successfully!", "OK");

            // Clear form
            RecipeTitle = string.Empty;
            Description = string.Empty;
            SelectedDifficulty = "Easy";
            CookingTimeMinutes = string.Empty;
            foreach (var cat in AvailableCategories)
                cat.IsSelected = false;
            foreach (var tag in AvailableTags)
                tag.IsSelected = false;
            Ingredients.Clear();
            Steps.Clear();
            SelectedImageSource = null;
            SelectedImagePath = null;
            NewIngredientName = string.Empty;
            NewIngredientQty = string.Empty;
            NewIngredientUnit = string.Empty;
            NewStepDescription = string.Empty;
            IsSaved = false;

            // Navigate back to home
            await Shell.Current.GoToAsync("//Home");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            SaveRecipeCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanSave() => !IsBusy;
}
