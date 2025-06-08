namespace ArcsomAssetManagement.Client.Pages;

public partial class AssetListPage : ContentPage
{
    public AssetListPage(AssetListPageModel model)
    {
        InitializeComponent();

        BindingContext = model;
    }
}