
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

        //PaginationPanel.Children.Clear();
        //PaginationHelper.Render(
        //        PaginationPanel,
        //        _model.Pagination.CurrentPage,
        //        _model.Pagination.TotalPages,
        //        _model.GoToPageCommand);

        //PaginationPanel.Children.Clear();

        //if (_model.Pagination.CurrentPage > 1)
        //{
        //    var buttonPrevious = new Button
        //    {
        //        Text = "Previous",
        //        Command = _model.GoToPageCommand,
        //        CommandParameter = _model.Pagination.CurrentPage - 1
        //    };
        //    PaginationPanel.Children.Add(buttonPrevious);
        //}

        //var startPage = Math.Max(_model.Pagination.CurrentPage - 2,1);
        //var endPage = Math.Min(_model.Pagination.CurrentPage + 2, _model.Pagination.TotalPages);

        //for (int i = startPage; i <= endPage; i++)
        //{
        //    var button = new Button
        //    {
        //        Text = i.ToString(),
        //        Command = _model.GoToPageCommand,
        //        CommandParameter = i,
        //        BackgroundColor = i == _model.Pagination.CurrentPage ? Colors.LightGray : Colors.Transparent,
        //        IsEnabled = !(i == _model.Pagination.CurrentPage)
        //    };
        //    PaginationPanel.Children.Add(button);
        //}

        //if (_model.Pagination.CurrentPage < _model.Pagination.TotalPages)
        //{
        //    var buttonNext = new Button
        //    {
        //        Text = "Next",
        //        Command = _model.GoToPageCommand,
        //        CommandParameter = _model.Pagination.CurrentPage + 1
        //    };
        //    PaginationPanel.Children.Add(buttonNext);
        //}
    }


    //private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == nameof(ManufacturerListPageModel.Pagination))
    //    {
    //        PaginationPanel.Children.Clear();
    //        PaginationHelper.Render(
    //            PaginationPanel,
    //            _model.Pagination.CurrentPage,
    //            _model.Pagination.TotalPages,
    //            _model.GoToPageCommand);
    //     }
    //}

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
      var searchText = e.NewTextValue;

        if (BindingContext is ManufacturerListPageModel viewModel)
        {
            viewModel.SearchText = searchText;
        }
    }

}