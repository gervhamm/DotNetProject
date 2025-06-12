namespace ArcsomAssetManagement.Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            SecureStorage.Remove("auth_token");
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}