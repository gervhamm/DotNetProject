using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using SQLite;
using ArcsomAssetManagement.Client.DTOs.Mapping;

namespace ArcsomAssetManagement.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Pages
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<ProductListPageModel>();
            builder.Services.AddSingleton<ManufacturerListPageModel>();
            builder.Services.AddTransientWithShellRoute<ManufacturerDetailPage, ManufacturerDetailPageModel>("manufacturer");
            builder.Services.AddTransientWithShellRoute<ProductDetailPage, ProductDetailPageModel>("product");
            builder.Services.AddSingleton<ManageMetaPageModel>();

            // Services
            //builder.Services.AddSingleton<SyncService<Manufacturer, ManufacturerDto>>(provider =>
            //{
            //    var sqliteConnection = provider.GetRequiredService<SQLiteAsyncConnection>();
            //    var onlineRepository = provider.GetRequiredService<ManufacturerOnlineRepository>();
            //    var mapper = provider.GetRequiredService<IMapper>();

            //    return new SyncService<Manufacturer, ManufacturerDto>(sqliteConnection, onlineRepository, mapper);
            //});
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();

            // Repositories
            builder.Services.AddTransient<SQLiteAsyncConnection>(provider =>
            {
                return new SQLiteAsyncConnection(Constants.DatabasePath);
            });

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ManufacturerRepository>();
            builder.Services.AddSingleton<ProductRepository>();

            return builder.Build();
        }
    }
}
