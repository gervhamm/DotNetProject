namespace ArcsomAssetManagement.Client.Pages;

public partial class AssetListPage : ContentPage
{
    public AssetListPage(AssetListPageModel model)
    {
        InitializeComponent();

        BindingContext = model;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue;

        if (BindingContext is ManufacturerListPageModel viewModel)
        {
            viewModel.SearchText = searchText;
        }
    }
}