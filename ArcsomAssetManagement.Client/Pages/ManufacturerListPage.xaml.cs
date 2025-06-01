namespace ArcsomAssetManagement.Client.Pages;

public partial class ManufacturerListPage : ContentPage
{
    private readonly ManufacturerListPageModel _model;
    public ManufacturerListPage(ManufacturerListPageModel model)
    {
        BindingContext = _model = model;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PaginationPanel.Children.Clear();

        for (int i = 1; i <= 3; i++)
        {
            var button = new Button
            {
                Text = i.ToString(),
                Command = _model.GoToPageCommand,
                CommandParameter = i,
                Margin = new Thickness(0, 0)
            };
            PaginationPanel.Children.Add(button);
        }
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