using ForkFeedMobile.Services;

namespace ForkFeedMobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;

        public App(AuthService authService)
        {
            InitializeComponent();

            _authService = authService;
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await _authService.TryRestoreSessionAsync();
        }
    }
}
