using Android.App;
using Android.Content.PM;
using Android.OS;
using ForkFeedMobile.Services;

namespace ForkFeedMobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ApplyStatusBarColor();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ApplyStatusBarColor();
        }

        private void ApplyStatusBarColor()
        {
            var statusBarService = IPlatformApplication.Current?.Services?.GetService<IStatusBarService>();
            var themeService = IPlatformApplication.Current?.Services?.GetService<ThemeService>();
            if (statusBarService != null)
            {
                var theme = themeService?.CurrentTheme ?? AppTheme.Unspecified;
                statusBarService.SetStatusBarColor(theme);
            }
        }
    }
}
