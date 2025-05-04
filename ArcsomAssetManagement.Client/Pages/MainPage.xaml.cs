using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.PageModels;

namespace ArcsomAssetManagement.Client.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}