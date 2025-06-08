namespace ArcsomAssetManagement.Client.Pages;

public partial class ProductDetailPage : ContentPage
{
    public ProductDetailPage(ProductDetailPageModel model)
    {
        InitializeComponent();

        BindingContext = model;

    }
}