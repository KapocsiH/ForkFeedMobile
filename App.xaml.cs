using ForkFeedMobile.Services;

namespace ForkFeedMobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly ThemeService _themeService;
        private readonly ConnectivityService _connectivityService;

        public App(AuthService authService, ThemeService themeService, ConnectivityService connectivityService)
        {
            InitializeComponent();

            _authService = authService;
            _themeService = themeService;
            _connectivityService = connectivityService;
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
            _connectivityService.Initialize();
            await _authService.TryRestoreSessionAsync();
        }
    }
}
