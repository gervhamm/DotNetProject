using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;

namespace ArcsomAssetManagement.Client
{
    public partial class AppShell : Shell
    {
        private readonly AuthService _authService;
        public AppShell()
        {
            InitializeComponent();

            _authService = Application.Current.Handler.MauiContext.Services.GetService<AuthService>();

            this.Navigating += OnShellNavigating;

            var currentTheme = Application.Current!.UserAppTheme;
            //ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
        }
        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            if (OperatingSystem.IsWindows())
                return;

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }

        private async void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            var route = e.Target.Location.OriginalString;

            // Check if the route is a login or register page
            if (!route.Contains("main"))
            {
                if (!await _authService.IsLoggedInAsync())
                {
                    await DisplaySnackbarAsync("Not logged in");
                    e.Cancel();
                    await Shell.Current.GoToAsync("//main");
                }
            }

            
        }
    }
}
