using ArcsomAssetManagement.Client.Data;
using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels
{
    public partial class ManageMetaPageModel : ObservableObject
    {
        //private readonly SeedDataService _seedDataService;

        [ObservableProperty]
        private ObservableCollection<Category> _categories = [];

        [ObservableProperty]
        private ObservableCollection<Tag> _tags = [];

        public ManageMetaPageModel()
        {
            //_seedDataService = seedDataService;
        }

        private async Task LoadData()
        {
        }

        [RelayCommand]
        private Task Appearing()
            => LoadData();

       

        [RelayCommand]
        private async Task Reset()
        {
            Preferences.Default.Remove("is_seeded");
            //await _seedDataService.LoadSeedDataAsync();
            Preferences.Default.Set("is_seeded", true);
            await Shell.Current.GoToAsync("//main");
        }
    }
}
