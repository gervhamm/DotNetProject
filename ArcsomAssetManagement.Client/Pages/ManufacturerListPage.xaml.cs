namespace ArcsomAssetManagement.Client.Pages;

public partial class ManufacturerListPage : ContentPage
{
    public ManufacturerListPage(ManufacturerListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
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