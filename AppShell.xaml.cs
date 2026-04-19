using ForkFeedMobile.Views;

namespace ForkFeedMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("RecipeDetail", typeof(RecipeDetailPage));
            Routing.RegisterRoute("CookingMode", typeof(CookingModePage));
            Routing.RegisterRoute("Register", typeof(RegisterPage));
            Routing.RegisterRoute("ForgotPassword", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("EditProfile", typeof(EditProfilePage));
            Routing.RegisterRoute("UserProfile", typeof(ProfilePage));
            Routing.RegisterRoute("RecipeBookDetails", typeof(RecipeBookDetailsPage));
        }
    }
}
