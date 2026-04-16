using System.ComponentModel;

namespace ForkFeedMobile.Services
{
    public class ThemeService : INotifyPropertyChanged
    {
        private const string ThemePreferenceKey = "app_theme";

        public event PropertyChangedEventHandler? PropertyChanged;

        public AppTheme CurrentTheme
        {
            get => Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
            private set
            {
                if (Application.Current != null)
                    Application.Current.UserAppTheme = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTheme)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThemeIcon)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThemeLabel)));
                UpdateStatusBar();
            }
        }

        public string ThemeIcon => CurrentTheme switch
        {
            AppTheme.Light => "??",
            AppTheme.Dark => "??",
            _ => "??"
        };

        public string ThemeLabel => CurrentTheme switch
        {
            AppTheme.Light => "Light",
            AppTheme.Dark => "Dark",
            _ => "System"
        };

        private readonly IStatusBarService? _statusBarService;

        public ThemeService()
        {
        }

        public ThemeService(IStatusBarService statusBarService)
        {
            _statusBarService = statusBarService;
        }

        public void UpdateStatusBar()
        {
            _statusBarService?.SetStatusBarColor(CurrentTheme);
        }

        public void Initialize()
        {
            LoadSavedTheme();
        }

        public void CycleTheme()
        {
            CurrentTheme = CurrentTheme switch
            {
                AppTheme.Unspecified => AppTheme.Light,
                AppTheme.Light => AppTheme.Dark,
                AppTheme.Dark => AppTheme.Unspecified,
                _ => AppTheme.Unspecified
            };

            Preferences.Set(ThemePreferenceKey, (int)CurrentTheme);
        }

        public void SetTheme(AppTheme theme)
        {
            CurrentTheme = theme;
            Preferences.Set(ThemePreferenceKey, (int)theme);
        }

        private void LoadSavedTheme()
        {
            var saved = Preferences.Get(ThemePreferenceKey, (int)AppTheme.Unspecified);
            if (Application.Current != null)
                Application.Current.UserAppTheme = (AppTheme)saved;
        }
    }
}
