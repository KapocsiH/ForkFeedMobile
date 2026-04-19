using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class CookingModeViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;

    [ObservableProperty]
    private int _recipeId;

    [ObservableProperty]
    private Recipe? _recipe;

    [ObservableProperty]
    private int _currentStepIndex;

    [ObservableProperty]
    private RecipeStep? _currentStep;

    [ObservableProperty]
    private string _stepProgress = string.Empty;

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _canGoForward;

    public CookingModeViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Title = "Cooking Mode";
    }

    partial void OnRecipeIdChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        var recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
        if (recipe == null || recipe.Steps.Count == 0) return;

        Recipe = recipe;
        CurrentStepIndex = 0;
        UpdateCurrentStep();
    }

    [RelayCommand]
    private void NextStep()
    {
        if (Recipe == null) return;
        if (CurrentStepIndex < Recipe.Steps.Count - 1)
        {
            CurrentStepIndex++;
            UpdateCurrentStep();
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStepIndex > 0)
        {
            CurrentStepIndex--;
            UpdateCurrentStep();
        }
    }

    private void UpdateCurrentStep()
    {
        if (Recipe == null) return;

        CurrentStep = Recipe.Steps[CurrentStepIndex];
        StepProgress = $"Step {CurrentStepIndex + 1} of {Recipe.Steps.Count}";
        CanGoBack = CurrentStepIndex > 0;
        CanGoForward = CurrentStepIndex < Recipe.Steps.Count - 1;
    }

    [RelayCommand]
    private async Task FinishCookingAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
