using CommunityToolkit.Maui.Views;

namespace ForkFeedMobile.Views;

public partial class NoInternetPopup : Popup
{
    public NoInternetPopup()
    {
        InitializeComponent();
        MessageLabel.Text = "Nincs internetel\u00E9r\u00E9s. Ellen\u0151rizd a h\u00E1l\u00F3zati kapcsolatot \u00E9s pr\u00F3b\u00E1ld \u00FAjra.";
        RetryButton.Text = "\u00DAjra";
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            await CloseAsync(true);
        }
        // If still no internet, do nothing — popup stays visible
    }
}
