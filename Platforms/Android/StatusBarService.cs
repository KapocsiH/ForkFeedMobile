using Android.OS;
using Android.Views;
using Microsoft.Maui.Platform;

namespace ForkFeedMobile.Services
{
    public class StatusBarService : IStatusBarService
    {
        public void SetStatusBarColor(AppTheme theme)
        {
            var activity = Platform.CurrentActivity;
            if (activity?.Window == null)
                return;

            var resolvedTheme = theme;
            if (resolvedTheme == AppTheme.Unspecified)
                resolvedTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;

            var window = activity.Window;

            if (resolvedTheme == AppTheme.Dark)
            {
                window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#191e24"));
                SetStatusBarIconsDark(window, false);
            }
            else
            {
                window.SetStatusBarColor(Android.Graphics.Color.White);
                SetStatusBarIconsDark(window, true);
            }
        }

        private static void SetStatusBarIconsDark(Android.Views.Window window, bool darkIcons)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var controller = window.InsetsController;
                if (controller != null)
                {
                    if (darkIcons)
                        controller.SetSystemBarsAppearance(
                            (int)WindowInsetsControllerAppearance.LightStatusBars,
                            (int)WindowInsetsControllerAppearance.LightStatusBars);
                    else
                        controller.SetSystemBarsAppearance(
                            0,
                            (int)WindowInsetsControllerAppearance.LightStatusBars);
                }
            }
            else
            {
#pragma warning disable CS0618
                var flags = window.DecorView.SystemUiVisibility;
                if (darkIcons)
                    flags |= (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                else
                    flags &= ~(StatusBarVisibility)SystemUiFlags.LightStatusBar;
                window.DecorView.SystemUiVisibility = flags;
#pragma warning restore CS0618
            }
        }
    }
}
