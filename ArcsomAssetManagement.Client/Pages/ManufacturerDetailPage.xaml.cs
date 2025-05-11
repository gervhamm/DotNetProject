namespace ArcsomAssetManagement.Client.Pages;

public partial class ManufacturerDetailPage : ContentPage
{
	public ManufacturerDetailPage(ManufacturerDetailPageModel model)
	{
		InitializeComponent();

        BindingContext = model;

    }
}