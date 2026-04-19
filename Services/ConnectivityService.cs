using CommunityToolkit.Maui.Views;
using ForkFeedMobile.Views;

namespace ForkFeedMobile.Services;

public class ConnectivityService
{
    private bool _isShowingPopup;

    public void Initialize()
    {
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        CheckAndShowPopup();
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            MainThread.BeginInvokeOnMainThread(() => CheckAndShowPopup());
        }
    }

    private async void CheckAndShowPopup()
    {
        if (_isShowingPopup)
            return;

        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            return;

        _isShowingPopup = true;

        try
        {
            while (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                var page = Application.Current?.MainPage;
                if (page == null)
                    break;

                var popup = new NoInternetPopup();
                await page.ShowPopupAsync(popup);
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                    break;
            }
        }
        finally
        {
            _isShowingPopup = false;
        }
    }
}
