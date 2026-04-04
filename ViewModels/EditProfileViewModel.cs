using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForkFeedMobile.Models;
using ForkFeedMobile.Services;

namespace ForkFeedMobile.ViewModels;

[QueryProperty(nameof(CurrentUsername), "username")]
[QueryProperty(nameof(CurrentBio), "bio")]
[QueryProperty(nameof(CurrentAvatarUrl), "avatarUrl")]
public partial class EditProfileViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _currentUsername = string.Empty;

    [ObservableProperty]
    private string _currentBio = string.Empty;

    [ObservableProperty]
    private string _currentAvatarUrl = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _bio = string.Empty;

    [ObservableProperty]
    private ImageSource? _avatarImageSource;

    [ObservableProperty]
    private string _formError = string.Empty;

    private string? _selectedImagePath;

    public EditProfileViewModel(IApiService apiService, AuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Edit Profile";
    }

    partial void OnCurrentUsernameChanged(string value) => Username = value;
    partial void OnCurrentBioChanged(string value) => Bio = value;

    partial void OnCurrentAvatarUrlChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            AvatarImageSource = ImageSource.FromUri(new Uri(value));
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a profile photo"
            });

            if (result != null)
            {
                _selectedImagePath = result.FullPath;
                AvatarImageSource = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied",
                "Gallery permission is required to select a photo.", "OK");
        }
        catch
        {
            // Silently ignore
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a profile photo"
            });

            if (result != null)
            {
                _selectedImagePath = result.FullPath;
                AvatarImageSource = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied",
                "Camera permission is required to take a photo.", "OK");
        }
        catch
        {
            // Silently ignore
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        FormError = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            FormError = "Username cannot be empty.";
            return;
        }

        IsBusy = true;

        try
        {
            string? newAvatarUrl = null;

            // Upload new profile image if one was selected
            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                using var stream = File.OpenRead(_selectedImagePath);
                var fileName = Path.GetFileName(_selectedImagePath);
                var uploadResult = await _apiService.UploadProfileImageAsync(stream, fileName);

                if (!uploadResult.IsSuccess || uploadResult.Data?.Url == null)
                {
                    FormError = uploadResult.ErrorMessage ?? "Failed to upload profile image.";
                    IsBusy = false;
                    return;
                }

                newAvatarUrl = uploadResult.Data.Url;
            }

            // Use the newly uploaded URL, or fall back to the existing one
            var effectiveAvatarUrl = newAvatarUrl ?? CurrentAvatarUrl;

            // Update profile data (including image URL if one was uploaded)
            var request = new UpdateProfileRequest
            {
                Username = Username.Trim(),
                Bio = Bio?.Trim(),
                ProfileImageUrl = effectiveAvatarUrl
            };

            var result = await _apiService.UpdateMyProfileAsync(request);

            if (!result.IsSuccess)
            {
                FormError = result.ErrorMessage ?? "Failed to update profile.";
                IsBusy = false;
                return;
            }

            // Update local user state so the Profile Page reflects changes immediately
            _authService.UpdateCurrentUser(
                displayName: Username.Trim(),
                bio: Bio?.Trim(),
                avatarUrl: effectiveAvatarUrl);

            IsBusy = false;

            var toast = Toast.Make("Changes saved ?", ToastDuration.Short, 14);
            await toast.Show();

            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            FormError = "An unexpected error occurred.";
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        bool hasChanges = Username != CurrentUsername
            || Bio != CurrentBio
            || !string.IsNullOrEmpty(_selectedImagePath);

        if (hasChanges)
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Discard Changes",
                "Are you sure you want to exit? All changes will be lost.",
                "Discard", "Stay");

            if (!confirm)
                return;
        }

        await Shell.Current.GoToAsync("..");
    }
}
