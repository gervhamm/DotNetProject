namespace ArcsomAssetManagement.Client.Pages;

public partial class AssetDetailPage : ContentPage
{
	public AssetDetailPage(AssetDetailPageModel model)
    {
        InitializeComponent();

        BindingContext = model;

    }
}