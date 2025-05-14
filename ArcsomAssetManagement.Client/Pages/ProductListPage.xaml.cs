namespace ArcsomAssetManagement.Client.Pages;

public partial class ProductListPage : ContentPage
{
	public ProductListPage(ProductListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue;

        if (BindingContext is ProductListPageModel viewModel)
        {
            viewModel.SearchText = searchText;
        }
    }
}