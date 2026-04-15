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

    [ObservableProperty]
    private string _recipeTitle = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _selectedDifficulty = "Easy";

    [ObservableProperty]
    private string _cookingTimeMinutes = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "Főétel";

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

    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<RecipeStep> Steps { get; } = new();
    public List<string> DifficultyOptions { get; } = new() { "Easy", "Medium", "Hard" };
    public List<string> CategoryOptions { get; } = new() { "Desszert", "Főétel", "Leves", "Reggeli", "Saláta" };

    public ObservableCollection<SelectableTag> AvailableTags { get; } = new()
    {
        new() { Name = "gyors" },
        new() { Name = "hagyományos" },
        new() { Name = "magyar" },
        new() { Name = "sültmentes" },
        new() { Name = "vegetáriánus" },
    };

    private static readonly Dictionary<string, int> CategoryIdMap = new()
    {
        ["Desszert"] = 3,
        ["Főétel"] = 2,
        ["Leves"] = 1,
        ["Reggeli"] = 4,
        ["Saláta"] = 5,
    };

    private static readonly Dictionary<string, int> TagIdMap = new()
    {
        ["gyors"] = 3,
        ["hagyományos"] = 4,
        ["magyar"] = 1,
        ["sültmentes"] = 5,
        ["vegetáriánus"] = 2,
    };

    public AddRecipeViewModel(RecipeService recipeService, AuthService authService)
    {
        _recipeService = recipeService;
        _authService = authService;
        Title = "Add Recipe";
    }

    [RelayCommand]
    private void ToggleTag(SelectableTag tag)
    {
        tag.IsSelected = !tag.IsSelected;
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

            var selectedCategoryId = CategoryIdMap.GetValueOrDefault(SelectedCategory, 0);
            var categoryIds = selectedCategoryId > 0 ? new List<int> { selectedCategoryId } : null;

            var tagIds = AvailableTags
                .Where(t => t.IsSelected && TagIdMap.ContainsKey(t.Name))
                .Select(t => TagIdMap[t.Name])
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
            SelectedCategory = "Főétel";
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
