using CommunityToolkit.Mvvm.ComponentModel;

namespace ForkFeedMobile.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;

    public bool IsNotLoading => !IsLoading;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    protected void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    protected void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
    }
}
