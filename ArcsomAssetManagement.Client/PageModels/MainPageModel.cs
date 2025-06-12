using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class MainPageModel : BasePageModel
{
    private readonly AuthRepository _authRepository;
    private readonly ModalErrorHandler _errorHandler;
    public MainPageModel(ModalErrorHandler errorHandler, AuthRepository authRepository, AuthService authService) : base(authService)
    {
        _authRepository = authRepository;
        _errorHandler = errorHandler;
        _authRepository = authRepository;
        //TODO: _seedDataService = seedDataService;
    }

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _displayLoggedIn;

    [ObservableProperty]
    private string _displayCurrentUser;

    [RelayCommand]
    public async Task TestAsync()
    {
        await Shell.Current.GoToAsync("//main");
        await AppShell.DisplaySnackbarAsync("test successful!");
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await AppShell.Current.DisplayAlert("Notification", "Please enter both username and password.", "OK");
                return;
            }
            try
            {
                var response = await _authRepository.LoginAsync(Username, Password);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                await CheckAuthAsync();
                DisplayLoggedIn = IsLoggedIn;
            }

            DisplayCurrentUser = CurrentUser ?? string.Empty;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await AppShell.Current.DisplayAlert("Notification", "Please enter both username and password.", "OK");
                return;
            }

            var response = await _authRepository.RegisterAsync(Username, Password);
            await AppShell.DisplaySnackbarAsync("Registration successful! You can now log in.");            
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await _authService.LogoutAsync();
            DisplayLoggedIn = false;
            DisplayCurrentUser = string.Empty;
            await AppShell.DisplaySnackbarAsync("You have been logged out.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private class TokenResponse
    {
        public string Token { get; set; }
    }


    //private bool _isNavigatedTo;
    //private bool _dataLoaded;
    //private readonly ProjectRepository _projectRepository;
    //private readonly TaskRepository _taskRepository;
    //private readonly CategoryRepository _categoryRepository;
    //private readonly ModalErrorHandler _errorHandler;
    //private readonly SeedDataService _seedDataService;

    //[ObservableProperty]
    //private List<CategoryChartData> _todoCategoryData = [];

    //[ObservableProperty]
    //private List<Brush> _todoCategoryColors = [];

    //[ObservableProperty]
    //private List<ProjectTask> _tasks = [];

    //[ObservableProperty]
    //private List<Project> _projects = [];

    //[ObservableProperty]
    //bool _isBusy;

    //[ObservableProperty]
    //bool _isRefreshing;

    //[ObservableProperty]
    //private string _today = DateTime.Now.ToString("dddd, MMM d");

    //public bool HasCompletedTasks
    //    => Tasks?.Any(t => t.IsCompleted) ?? false;



    //    private async Task LoadData()
    //    {
    //        try
    //        {
    //            IsBusy = true;

    //            Projects = await _projectRepository.ListAsync();

    //            var chartData = new List<CategoryChartData>();
    //            var chartColors = new List<Brush>();

    //            var categories = await _categoryRepository.ListAsync();
    //            foreach (var category in categories)
    //            {
    //                chartColors.Add(category.ColorBrush);

    //                var ps = Projects.Where(p => p.CategoryID == category.ID).ToList();
    //                int tasksCount = ps.SelectMany(p => p.Tasks).Count();

    //                chartData.Add(new(category.Title, tasksCount));
    //            }

    //            TodoCategoryData = chartData;
    //            TodoCategoryColors = chartColors;

    //            Tasks = await _taskRepository.ListAsync();
    //        }
    //        finally
    //        {
    //            IsBusy = false;
    //            OnPropertyChanged(nameof(HasCompletedTasks));
    //        }
    //    }

    //    private async Task InitData(SeedDataService seedDataService)
    //    {
    //        bool isSeeded = Preferences.Default.ContainsKey("is_seeded");

    //        if (!isSeeded)
    //        {
    //            await seedDataService.LoadSeedDataAsync();
    //        }

    //        Preferences.Default.Set("is_seeded", true);
    //        await Refresh();
    //    }

    //    [RelayCommand]
    //    private async Task Refresh()
    //    {
    //        try
    //        {
    //            IsRefreshing = true;
    //            await LoadData();
    //        }
    //        catch (Exception e)
    //        {
    //            _errorHandler.HandleError(e);
    //        }
    //        finally
    //        {
    //            IsRefreshing = false;
    //        }
    //    }

    //    [RelayCommand]
    //    private void NavigatedTo() =>
    //        _isNavigatedTo = true;

    //    [RelayCommand]
    //    private void NavigatedFrom() =>
    //        _isNavigatedTo = false;

    //    [RelayCommand]
    //    private async Task Appearing()
    //    {
    //        if (!_dataLoaded)
    //        {
    //            await InitData(_seedDataService);
    //            _dataLoaded = true;
    //            await Refresh();
    //        }
    //        // This means we are being navigated to
    //        else if (!_isNavigatedTo)
    //        {
    //            await Refresh();
    //        }
    //    }
}