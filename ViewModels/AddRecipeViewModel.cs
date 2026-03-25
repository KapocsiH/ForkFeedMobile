using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

public partial class AddRecipeViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;

    [ObservableProperty]
    private string _recipeTitle = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _selectedDifficulty = "Easy";

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
    public List<string> DifficultyOptions { get; } = new() { "Easy", "Medium", "Hard" };

    public AddRecipeViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Title = "Add Recipe";
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

    [RelayCommand]
    private async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(RecipeTitle))
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter a recipe title.", "OK");
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

        IsBusy = true;

        var recipe = new Recipe
        {
            Title = RecipeTitle.Trim(),
            Description = Description.Trim(),
            Difficulty = SelectedDifficulty,
            ImageUrl = SelectedImagePath ?? "https://images.unsplash.com/photo-1495521821757-a1efb6729352?w=600",
            TimeMinutes = Steps.Count * 10,
            Ingredients = Ingredients.ToList(),
            Steps = Steps.ToList()
        };

        await _recipeService.AddRecipeAsync(recipe);

        IsBusy = false;
        IsSaved = true;

        await Shell.Current.DisplayAlert("Success", "Recipe saved!", "OK");

        RecipeTitle = string.Empty;
        Description = string.Empty;
        SelectedDifficulty = "Easy";
        Ingredients.Clear();
        Steps.Clear();
        SelectedImageSource = null;
        SelectedImagePath = null;
        IsSaved = false;
    }
}
