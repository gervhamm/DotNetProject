using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.Input;

namespace ArcsomAssetManagement.Client.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}