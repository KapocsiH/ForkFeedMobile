using CommunityToolkit.Maui.Views;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.Views;

public partial class ReportPopup : Popup
{
    private readonly IApiService _apiService;
    private readonly string _targetType;
    private readonly int _targetId;

    public ReportPopup(IApiService apiService, string targetType, int targetId)
    {
        InitializeComponent();
        _apiService = apiService;
        _targetType = targetType;
        _targetId = targetId;
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        var reason = ReasonEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(reason))
        {
            ShowError("Please enter a reason.");
            return;
        }

        SetLoading(true);
        ErrorLabel.IsVisible = false;

        var request = new CreateReportRequest
        {
            TargetType = _targetType,
            TargetId = _targetId,
            Reason = reason
        };

        var result = await _apiService.CreateReportAsync(request);

        SetLoading(false);

        if (result.IsSuccess)
        {
            await CloseAsync(true);
        }
        else
        {
            ShowError(result.ErrorMessage ?? "Failed to submit report.");
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

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        SubmitButton.IsEnabled = !isLoading;
        ReasonEditor.IsEnabled = !isLoading;
    }
}
