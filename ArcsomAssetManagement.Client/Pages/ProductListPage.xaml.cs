namespace ArcsomAssetManagement.Client.Pages;

public partial class ProductListPage : ContentPage
{
	public ProductListPage(ProductListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}