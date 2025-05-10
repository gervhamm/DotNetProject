namespace ArcsomAssetManagement.Client.Pages;

public partial class ManufacturerListPage : ContentPage
{
    public ManufacturerListPage(ManufacturerListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}