using ForkFeedMobile.Services;

namespace ForkFeedMobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly ThemeService _themeService;

        public App(AuthService authService, ThemeService themeService)
        {
            InitializeComponent();

            _authService = authService;
            _themeService = themeService;
            _themeService.Initialize();
            MainPage = new AppShell();

            RequestedThemeChanged += (s, e) =>
            {
                if (_themeService.CurrentTheme == AppTheme.Unspecified)
                    _themeService.UpdateStatusBar();
            };
        }

        protected override async void OnStart()
        {
            base.OnStart();
            _themeService.UpdateStatusBar();
            await _authService.TryRestoreSessionAsync();
        }
    }
}
