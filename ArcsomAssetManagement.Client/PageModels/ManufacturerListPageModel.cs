using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ManufacturerListPageModel : ObservableObject
{
    private readonly ManufacturerRepository _manufacturerRepository;

    [ObservableProperty]
    private List<Manufacturer> _manufacturers = [];

    public ManufacturerListPageModel(ManufacturerRepository manufacturerRepository)
    {
        _manufacturerRepository = manufacturerRepository;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        Manufacturers = await _manufacturerRepository.ListAsync();
    }
}
