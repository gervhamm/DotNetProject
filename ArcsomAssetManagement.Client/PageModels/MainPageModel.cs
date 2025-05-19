using ArcsomAssetManagement.Client.DTOs.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Text;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class MainPageModel : ObservableObject//, IProjectTaskPageModel TODO: BaseViewModel met default reauste header with bearer
{
    private readonly HttpClient _httpClient;


    //public MainPageModel(SeedDataService seedDataService, ProjectRepository projectRepository,
    //    TaskRepository taskRepository, CategoryRepository categoryRepository, ModalErrorHandler errorHandler)
    public MainPageModel(ModalErrorHandler errorHandler)
    {
        _httpClient = new HttpClient();

        //_projectRepository = projectRepository;
        //_taskRepository = taskRepository;
        //_categoryRepository = categoryRepository;
        //_errorHandler = errorHandler;
        //_seedDataService = seedDataService;
    }

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    public async Task Test()
    {
        await Shell.Current.GoToAsync("//projects");
        //await AppShell.DisplaySnackbarAsync("test successful!"); TODO: for android use toast
    }

    //[RelayCommand]
    //public async Task LoginAsync()
    //{
    //    await AppShell.DisplaySnackbarAsync("Login successful!");
    //}
    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var loginDto = new LoginDto
            {
                Email = Email,
                Password = Password
            };

            var json = JsonConvert.SerializeObject(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // TODO: var response = await _httpClient.PostAsync("https://yourapiurl.com/api/auth/login", content);

            //if (response.IsSuccessStatusCode)
            //{
            //    var result = await response.Content.ReadAsStringAsync();
            //    var token = JsonConvert.DeserializeObject<TokenResponse>(result).Token;

            //    await SecureStorage.SetAsync("auth_token", token);

            //    await AppShell.DisplayToastAsync("Login successful!");

            //    await Shell.Current.GoToAsync("//home");
            //}
            //else
            //{
            //    await AppShell.DisplaySnackbarAsync("Login failed. Please check your credentials.");
            //}
            // TODO: Remove

            await Shell.Current.GoToAsync("//projects");
        }
        catch (Exception ex)
        {
            await AppShell.Current.DisplayAlert("Notification", $"An error occurred: {ex.Message}", "OK");
            //await AppShell.DisplaySnackbarAsync($"An error occurred: {ex.Message}");
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

    //    [RelayCommand]
    //    private Task TaskCompleted(ProjectTask task)
    //    {
    //        OnPropertyChanged(nameof(HasCompletedTasks));
    //        return _taskRepository.SaveItemAsync(task);
    //    }

    [RelayCommand]
    private Task AddTask()
        => Shell.Current.GoToAsync($"task");

    //    [RelayCommand]
    //    private Task NavigateToProject(Project project)
    //        => Shell.Current.GoToAsync($"project?id={project.ID}");

    //    [RelayCommand]
    //    private Task NavigateToTask(ProjectTask task)
    //        => Shell.Current.GoToAsync($"task?id={task.ID}");

    //    [RelayCommand]
    //    private async Task CleanTasks()
    //    {
    //        var completedTasks = Tasks.Where(t => t.IsCompleted).ToList();
    //        foreach (var task in completedTasks)
    //        {
    //            await _taskRepository.DeleteItemAsync(task);
    //            Tasks.Remove(task);
    //        }

    //        OnPropertyChanged(nameof(HasCompletedTasks));
    //        Tasks = new(Tasks);
    //        await AppShell.DisplayToastAsync("All cleaned up!");
    //    }
}