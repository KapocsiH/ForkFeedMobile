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
    private string _selectedCategory = "Main Course";

    [ObservableProperty]
    private string _newTag = string.Empty;

    [ObservableProperty]
    private string _newIngredientName = string.Empty;

    [ObservableProperty]
    private string _newIngredientQty = string.Empty;

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
    public ObservableCollection<string> Tags { get; } = new();
    public List<string> DifficultyOptions { get; } = new() { "Easy", "Medium", "Hard" };
    public List<string> CategoryOptions { get; } = new() { "Dessert", "Main Course", "Soup", "Breakfast", "Salad" };

    public AddRecipeViewModel(RecipeService recipeService, AuthService authService)
    {
        _recipeService = recipeService;
        _authService = authService;
        Title = "Add Recipe";
    }

    [RelayCommand]
    private void AddTag()
    {
        if (string.IsNullOrWhiteSpace(NewTag)) return;

        var tag = NewTag.Trim().TrimStart('#');
        if (string.IsNullOrWhiteSpace(tag)) return;

        var formatted = $"#{tag}";
        if (!Tags.Contains(formatted))
            Tags.Add(formatted);

        NewTag = string.Empty;
    }

    [RelayCommand]
    private void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }

    [RelayCommand]
    private void AddIngredient()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientName)) return;

        Ingredients.Add(new Ingredient
        {
            Name = NewIngredientName.Trim(),
            Quantity = NewIngredientQty.Trim()
        });

        NewIngredientName = string.Empty;
        NewIngredientQty = string.Empty;
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

            var (success, recipeId, error) = await _recipeService.CreateRecipeAsync(
                RecipeTitle.Trim(),
                Description.Trim(),
                SelectedDifficulty,
                preparationTime,
                Ingredients.ToList(),
                Steps.ToList(),
                SelectedImagePath);

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
            SelectedCategory = "Main Course";
            NewTag = string.Empty;
            Tags.Clear();
            Ingredients.Clear();
            Steps.Clear();
            SelectedImageSource = null;
            SelectedImagePath = null;
            NewIngredientName = string.Empty;
            NewIngredientQty = string.Empty;
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
